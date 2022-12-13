
[<AutoOpen>]
module NewGameProject.Orbitals.LineDrawer
open Godot


///Takes a list and uses filter to remove 
let removeConsecutiveDuplicates pointsHistory= 
    let mutable lastPoint=null
    pointsHistory|>Array.filter(fun x-> 
        if x<>lastPoint then 
            lastPoint<-x 
            true
        else false )
//TODO: I should probably make this run from the orbit master but for now this will do
///Designed to be the child of the orbitalBody. draws a line for it

type OrbitalLineDrawer()=
    inherit ImmediateGeometry()
    let mutable points=Array.empty
    let mutable parentDead=false;
    let newLineData (orbitParent:OrbitalBodyNode) (newState:NewGameProject.OrbitalPhysics.OrbitalBody[][])=
        if parentDead= false then
            let id=orbitParent.GetInstanceId()
            if newState.Length >0 && newState[0]|>Array.tryFind(fun x->x.ID=id)|>Option.isSome then
                //TODO:This is probably pretty slow and could be fixed by inverting the arrays so that it is entity[timesteps[]] rather thantimestep[entity[]]
                let pointsHistory=newState|>Array.map(fun x->(x|>Array.find(fun x->x.ID=id)).Transform)
                //TODO We could include colouring based on velocity
                //Dedup
                //let mutable lastPoint
                //pointsHistory|>Array.filter(x->)
                points<-pointsHistory
            else GD.Print("Trying to draw line for body that is not in the simulation")

    override x._Ready()=
        //We do this becuase we want the transform to always be relative to the origin
        x.SetAsToplevel(true);
        x.Translation<- new Vector3(0f,0f,0f)

        let orbitParent= x.GetParent():?>OrbitalBodyNode
        let orbitMaster= OrbitMaster.getOrbitMasterInstance x
        
        orbitMaster.FinishedCalculatingStates.Add (newLineData orbitParent)
    override  x._Process(delta)=
        base._Process(delta);

        x.Clear();

        x.Begin(Mesh.PrimitiveType.Lines);

        points|>Seq.iter(x.AddVertex)

        x.End();
    override x._ExitTree()=
        parentDead<-true
        ()