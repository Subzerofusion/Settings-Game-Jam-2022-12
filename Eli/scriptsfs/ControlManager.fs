module NewGameProject.Control
open System
open RayCast
open Orbitals.Nodes
open Godot
open Calc
open Godot.Collections
open FSharp.Control.Reactive
open InputHandler
(*
There are 2 states for placement. 
Placment can either be around a planet or from a particular position


*)
type NotifyerValue<'a when 'a:equality>(value:'a)=
    let mutable value=value
    let event= Event<'a>()
    member x.Value =value
    member x.Set(newValue) =
        if value<>newValue then
            value<-newValue
            event.Trigger(value)
    member val Event=event.Publish
    member x.ManuallyTrigger()=event.Trigger(value)




type PlacementState={
    VelocityMagnitude:float32
    Mass:float32
}

type StartSelection=
|Planet of Vector3
|Position of Vector3

type ObjectPlacement={
    StartSelection:StartSelection option
}
type Placement={
    Velocity:Vector3
    Position:Vector3
}
let ensureNotZero(dir:Vector3)=
    if dir.LengthSquared() = 0f then Vector3.Left
    else dir
module ObjectPlacement=
    //Calcultes the velocity the object should be simulated with
    let calcVelocityAndPos (placement:ObjectPlacement) (velocityMagnitude:float32) (mousePosition:Vector3)=
        match placement.StartSelection.Value with
        |Planet planetPos->
            {  Velocity=(mousePosition.DirectionTo(planetPos)|>Calc.rotateVector90)*velocityMagnitude;
                Position=mousePosition}
        |Position position -> 
        {   Velocity=position.DirectionTo(mousePosition)*velocityMagnitude;
            Position=position}

        
//If the objectPlacement is null the the placement hasn't started
type MouseState=
|PlacingObject of OrbitalBodyNode*ObjectPlacement
|Selecting 
|Deleting

type ControlManager() as x=
    inherit Node()

    let placementIndicator:Lazy<Spatial>= lazy(x.GetNode("Camera/PlacementIndicator"):?>Spatial)
    let camera:Lazy<Camera>= lazy(x.GetNode("Camera"):?>Camera)
    let orbitMaster:Lazy<OrbitMaster>=lazy(OrbitMaster.getOrbitMasterInstance(x))

    ///LevelConditions
    [<Export>]
    let mutable maxPlanets=1
    [<Export>]
    let mutable targetPoints=2
    [<Export>]
    let mutable nextLevelPath=""


    let mutable createdPlanets=0
    let mutable hitTargets=0

    let mutable currentSelectionNode:Option<Orbitals.Nodes.OrbitalBodyNode>=None
    let mouseState=NotifyerValue(MouseState.Selecting)
    let mutable velocityMagnitude=100f
    let placementState= NotifyerValue({Mass=100f;VelocityMagnitude=100f})
    ///Used to save our numbers for when we pickup an object
    let mutable lastPlacementState:PlacementState option=None
    let orbitalNode=lazy(GD.Load<PackedScene>("res://Orbiter.tscn"))
    let resetEvent=Event<unit>()


    let getPlacementPoint()=
        let res=rayCastFromMouse camera.Value 3000f (2u)
        match res with 
        |None-> Vector3.Zero//GD.Print "Raycast didn't hit anything"
        |Some cast->
            let point=Vector3(cast.position.x,0f,cast.position.z)
            placementIndicator.Value.GlobalTranslation<-point
            point

    let handleDeselection (placementPoint:Vector3) =
        match mouseState.Value with 
        |PlacingObject (node,placement) ->
            PlacingObject(node,{placement with StartSelection= Some (Position placementPoint)})
        |Selecting->Selecting
        |Deleting->Selecting
    let handleSelection (placementPoint:Vector3) (node:OrbitalBodyNode) =
        match mouseState.Value with 
        |PlacingObject (node,placement) ->
            PlacingObject(node,{placement with StartSelection= Some (Planet placementPoint)})
        |Selecting->Selecting
        |Deleting-> 
            node.QueueFree()
            Selecting
    let handleChangedSelection (newSelection:OrbitalBodyNode option)=
        if currentSelectionNode=newSelection then
            ()
        else
            GD.Print (sprintf "Current selection changing to node: %A" x)
            currentSelectionNode |>Option.iter (fun x->x.Deselect())
            newSelection |>Option.iter (fun x->x.Select())
            currentSelectionNode<-newSelection   

    let handlePlacing (node:OrbitalBodyNode) placeData  placePoint=
        node.Mass<-placementState.Value.Mass
        
        //This is the "not yet started placement" ,we should make the planet follow
        match placeData.StartSelection with
        |None->
            node.Translation<-placePoint
            node.SetStartingVelocity (Vector3.Left*placementState.Value.VelocityMagnitude)
        |Some x->
            let placement=ObjectPlacement.calcVelocityAndPos placeData  placementState.Value.VelocityMagnitude placePoint
            node.SetStartingVelocity placement.Velocity
            node.Translation<-placement.Position
            
        

    let pickupNode(node:OrbitalBodyNode):MouseState=
        if not node.PlayerNode then failwith "tried to pickup a non-player node"
        GD.Print($"picking up node: {node.Name}")
        node.CollisionLayer<-4u

        lastPlacementState<-Some placementState.Value
        //Set the placement state to be that of the object we are picking up
        placementState.Set {Mass=node.Mass; VelocityMagnitude= node.StartingVelocity.Length()}

        PlacingObject  (node,{StartSelection=None})

    let placeBody(mouseState):MouseState=
        match mouseState with
        |PlacingObject (node,_)->
            match lastPlacementState with
            |None ->()
            |Some state ->  
                placementState.Set state
                lastPlacementState<- None
            node.CollisionLayer<-1u
            Selecting
        |Selecting->Selecting
        |Deleting->Selecting

    let startPlacing():MouseState=
        createdPlanets<-1
        placeBody(mouseState.Value)|>ignore
        let newNode=orbitalNode.Value.Instance():?>OrbitalBodyNode
        newNode.PlayerNode<-true
        //stop it from colliding with the mouse untill we actually place it
        newNode.CollisionLayer<-4u
        x.GetNode("/root/Spatial").AddChild(newNode)
        GD.Print("Made new orbital node with id "+newNode.GetInstanceId().ToString())
        //TODO I may want to add this as a child of something
        PlacingObject  (newNode,{StartSelection=None})

    let inputMappings= {
        velocityUp=(fun x->  placementState.Set { placementState.Value with VelocityMagnitude= placementState.Value.VelocityMagnitude*1.05f+0.001f})>>(fun x->GD.Print "velocity UP")
        velocityDown=(fun x-> placementState.Set { placementState.Value with VelocityMagnitude= placementState.Value.VelocityMagnitude/1.05f+0.001f})
        massUp=(fun x-> placementState.Set { placementState.Value with Mass=placementState.Value.Mass*1.05f+0.001f})
        massDown=(fun x-> placementState.Set { placementState.Value with Mass= placementState.Value.Mass/1.05f+0.001f})
        placeBody=(fun _-> mouseState.Set (placeBody mouseState.Value))
        play=(fun _-> resetEvent.Trigger())
    }
    
    let handleReset()=
        orbitMaster.Value.PlayPause()
        hitTargets <-0  
    let switchLevels(orbitalMaster:OrbitMaster)=
        orbitalMaster.ChangeScenes()
        
        x.GetTree().ChangeScene(nextLevelPath)|>ignore
    let handleTargetHit()=
        hitTargets<- hitTargets+1
        if targetPoints=hitTargets then
            //TODO probably shouldn't ignore this
            async {
                do! Async.Sleep 3000
                switchLevels(orbitMaster.Value)
            }|>Async.Start
        ()
    ///Should contain all envents related to selection and deselection. 
    ///Deselection is an event with a None value
    let currentSelectionChanged=Event<Orbitals.Nodes.OrbitalBodyNode option>()
    member x.PlacementState=placementState
    member x.MouseState=mouseState
    member val targetHit=Event<unit>()
    ///Emitted when the game gets reset
    member x.Reset=resetEvent.Publish
    override x._Ready()=
        let orbitMaster=orbitMaster.Value
        let subscribeToSelectionChanges (newBody:OrbitalBodyNode)=
            GD.Print (sprintf "Adding Orbital node %A to list of selectable nodes" newBody)
            newBody.SelectedEvent.Add(fun ()->currentSelectionChanged.Trigger(Some newBody))
        x.Reset.Add handleReset
        x.targetHit.Publish.Add handleTargetHit
        //process existing nodes 
        orbitMaster.OrbitalBodies|>Seq.iter subscribeToSelectionChanges
        //add any new nodes
        orbitMaster.NewOrbitalBody.Add subscribeToSelectionChanges
        placementState.ManuallyTrigger()
    override x._Input(event)=

        let placementPoint=getPlacementPoint()
        InputHandler.handleInputs inputMappings event
        let nextMouseState=
            if event.IsActionPressed(Actions.select) then
            
                let castResult=rayCastFromMouse camera.Value 3000f 1u
                match castResult with
                |None->
                    currentSelectionChanged.Trigger(None)
                    handleChangedSelection(None)
                    handleDeselection(placementPoint)
                |Some castResult->
                    let className =castResult.collider.GetClass()
                    let id=castResult.collider.GetInstanceId()
                    GD.Print($"collided with className {className} id {id}")
                    //We can assume anything we collide with is our orbiters child node
                    let orbitalNode=(castResult.collider:?>OrbitalBodyNode)
                    if Input.IsActionPressed("ctrl") then GD.Print "trying to pickup node"
                    if Input.IsActionPressed("ctrl") && orbitalNode.PlayerNode then
                        pickupNode orbitalNode
                    else
                        handleChangedSelection(Some orbitalNode)
                        handleSelection placementPoint orbitalNode
            else if event.IsActionPressed(Actions.option1) && createdPlanets<maxPlanets then
                //Try place if necissary
                startPlacing()
            
            else mouseState.Value
        mouseState.Set nextMouseState

    override x._Process(delta)=
        match mouseState.Value with
        |PlacingObject (node,place)->
            //TODO This should only trigger
            let point=getPlacementPoint()
            handlePlacing node place point
            orbitMaster.Value.UpdateNode node 
            orbitMaster.Value.Recalculate()
            
        | Selecting -> ()
        | Deleting -> ()
        //TODO come up with a control scheme
        ()
        