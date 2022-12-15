[<AutoOpen>]
module NewGameProject.Orbitals.Nodes
open NewGameProject
open Godot
open NewGameProject.OrbitalPhysics
open System.Collections.Generic
// let rec findNodesWithScript(scriptId : RID) (foundNodes) (node: Node) =
//     let script=node.GetScript()
//     //This should be the id of the individual script
//     let thisScriptId=script.GetRid()
//     let newNodes=
//         if thisScriptId=scriptId then 
//             node::foundNodes
//         else foundNodes
//     let children= node.GetChildren()
//     if children.Count=0 then newNodes
//     else 
//         let res=children |>Seq.collect (findNodesWithScript scriptId newNodes)|>Seq.toList
//         res

// let rec findNodesWithScriptFast(scriptId : RID) (foundNodes:ResizeArray<Node>) (node: Node) =
//     let script=node.GetScript().As<Script>()
//     //This should be the id of the individual script
//     let thisScriptId=script.GetRid()
//     if thisScriptId= scriptId then foundNodes.Add(node)
//     for nextNode in node.GetChildren() do
//         findNodesWithScriptFast scriptId foundNodes nextNode
module OrbitalBodyNodePaths=
    let outlineNode="mesh/outline"


let toggleOutline(node:Node) enable=
    let outlineNode=node.GetNode(OrbitalBodyNodePaths.outlineNode):?>Spatial
    outlineNode.Visible<-enable

    
    

type OrbitalBodyNode()as this=
    inherit Godot.RigidBody()
    let mutable bodies=ResizeArray<OrbitalBodyNode>()
    let selected = Event<unit>()
    let orbitMaster:Lazy<OrbitMaster> = lazy( OrbitMaster.getOrbitMasterInstance this)
    // member private x._on_Orbiter_input_event(camera: obj, event: obj, position:Vector3, normal:Vector3, shape_idx:int)=
    //     if Input.IsActionPressed(Actions.select) then 
    //         toggleOutline x true
    //         GD.Print(sprintf "Body with node id %i enabling selection" (x.GetInstanceId()))
    //         selected.Trigger()
    [<Export>]
    let mutable startingVelocity=Vector3()
    [<Export>]
    let mutable IsBall=false
    ///Triggers when this object is selected
    member x.SelectedEvent=selected.Publish
    
    member x.StartingVelocity=startingVelocity 
    member x.SetStartingVelocity velocity=startingVelocity <-velocity
    member val PlayerNode=false with get,set
    member x.Deselect()=
        GD.Print(sprintf "Body with node id %i disabling selection" (x.GetInstanceId()))
        toggleOutline x false
    
    member x.Select()=
        GD.Print(sprintf "Body with node id %i enabling selection" (x.GetInstanceId()))
        toggleOutline x true

    member val OrbitalID=System.Guid.NewGuid()

    //Init 
    override this._Ready()=
        GD.Print (sprintf"Starting to init orbital body. velocity=%A" startingVelocity)
        //find the orbitmaster singleton
        let orbitMaster= orbitMaster.Value
        //Add self to make self controlled by master
        orbitMaster.AddOrbitalNode this
        bodies<-orbitMaster.OrbitalBodies

        GD.Print "finished  init orbital body"
        ()
    override this._ExitTree()=
        orbitMaster.Value.DeleteOrbitalBody this
    

and OrbitMaster()=
    inherit Godot.RigidBody()
    
    let mutable dt=0.5f
//    let mutable timeSteps=Array.zeroCreate 3600
    let mutable resetTimeSteps=false
    let mutable play=false
    let mutable calculateStates=true
    let mutable restartSimulation=false

    let mutable startingEntites= Dictionary()
    let mutable orbitalStates:OrbitalBody[][]=Array.empty
    let mutable step=0
    [<Export>]
    let mutable maxTimeSteps=1500 
    

    let newOrbitalBody= Event<OrbitalBodyNode>()
    let deletedOrbitalBody= Event<OrbitalBodyNode>()

    let finishedCalculatingStates = new Event<OrbitalBody[][]>()
    let orbitalNodeToOrbitalData (node:OrbitalBodyNode) :OrbitalBody = 
        {Velocity=node.StartingVelocity;Transform= new Vector3( node.Transform.origin.x,0f,node.Transform.origin.z);OrbitalData={Mass=node.Mass};ID=node.GetInstanceId();acceleration=Vector3()}
    
    let applyOrbitalData(data:OrbitalBody) (node:OrbitalBodyNode)=
        //TODO: find out how slow getinstanceID is
        if node.GetInstanceId()<> data.ID then 
            failwithf "trying to set an orbital node with data that was not meant for it"
        node.Translation <-data.Transform
        node.LinearVelocity<-data.Velocity

    static member getOrbitMasterInstance (this:Node)=
        this.GetNode<OrbitMaster>("/root/OrbitMaster");
    //should be ste from base class
    member val OrbitalBodies= ResizeArray<OrbitalBodyNode>() with get,set
    member this.PlayPause()=
        GD.Print "switching from play to pause"
        if play then 
            restartSimulation<-true
            resetTimeSteps<-true
        play<-not play
    member this.NewOrbitalBody=newOrbitalBody.Publish
    member this.DeletedOrbitalBody=deletedOrbitalBody.Publish
    member this.ChangeScenes()=
        play<-false
        restartSimulation<-true
        resetTimeSteps<-true
        startingEntites.Clear()
        this.OrbitalBodies.Clear()
    // [<CLIEvent>]
    member this.FinishedCalculatingStates = finishedCalculatingStates.Publish
    member this.Recalculate()= resetTimeSteps<-true
    ///We need the velocity because nodes seeem to not accept velocity when i set it
    member this.UpdateNode(node:OrbitalBodyNode) =
        startingEntites[node.GetInstanceId()]<- (orbitalNodeToOrbitalData node)

    member this.AddOrbitalNode(node:OrbitalBodyNode)=
        GD.Print(sprintf "OrbitMaster:Adding orbital node with name %s" node.Name)
        GD.Print(sprintf "Node params: starting velocity= %A position =%A  " node.StartingVelocity node.Translation)
        this.OrbitalBodies.Add(node)
        let nodeId=node.GetInstanceId()
        startingEntites.Add(nodeId,orbitalNodeToOrbitalData node )
        resetTimeSteps<-true
        newOrbitalBody.Trigger(node)
        GD.Print(sprintf "starting entity= %A" (startingEntites.[nodeId]))
        ()

    member this.DeleteOrbitalBody(node:OrbitalBodyNode)=
        GD.Print(sprintf "OrbitMaster:Deleting orbital node with name %s" node.Name)
        this.OrbitalBodies.Remove(node)|>ignore
        startingEntites.Remove(node.GetInstanceId())
        resetTimeSteps<-true
        deletedOrbitalBody.Trigger(node)
        ()

    override this._Ready()=
        GD.Print "Initialising orbital Master"
        
        orbitalStates <-Array.zeroCreate maxTimeSteps
        
    
        ()
    override this._Process(delta)=

        ()
    
    override this._IntegrateForces(delta) =
        dt<-this.GetPhysicsProcessDeltaTime()
        if resetTimeSteps then
            GD.Print("OrbitMaster-Timestamp reset")
            orbitalStates<- Array.zeroCreate maxTimeSteps
            step<-0
            resetTimeSteps<-false
        if play&& orbitalStates[0]|>isNull|>not&& step<maxTimeSteps then
            let entites=orbitalStates[step]
            // GD.Print("applying states")
            

            (entites|>Seq.sortBy(fun x->x.ID),this.OrbitalBodies|>Seq.sortBy(fun x->x.GetInstanceId()))||> Seq.iter2(fun x y-> applyOrbitalData x y )
            step<- step+1
        if calculateStates && startingEntites.Count>0 then
            if (Array.last orbitalStates) = null then
                GD.Print($"OrbitMaster-Calculating state for {startingEntites.Count} entities ")
                let startingEntites= startingEntites|>Seq.map(fun x->x.Value)|>Seq.toArray
                orbitalStates<- accumulateSteps startingEntites (int(dt*1000.0f)) maxTimeSteps
                GD.Print($"OrbitMaster- Finished Calculating state ")
                finishedCalculatingStates.Trigger(orbitalStates)
        if restartSimulation then
            //reset to
            (startingEntites|>Seq.sortBy(fun x->x.Key),this.OrbitalBodies|>Seq.sortBy(fun x->x.GetInstanceId()))||> Seq.iter2(fun x y-> applyOrbitalData x.Value y )
            restartSimulation<-false
        ()
