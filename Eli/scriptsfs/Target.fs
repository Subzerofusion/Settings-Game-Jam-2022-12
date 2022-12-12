
module NewGameProject.Target
open System
open RayCast
open Orbitals.Nodes
open Godot
open Calc
open Godot.Collections
open Control

type State=
|NotHit=0
|Hit=1


type Target() as this=
    inherit Area()
    let controlManager=lazy(this.GetNode<ControlManager>("/root/Spatial/ControlManager"))    
    let Hit=lazy(this.GetNode<MeshInstance>("Hit"))
    let NotHit=lazy(this.GetNode<MeshInstance>("NotHit"))
    let mutable state=State.NotHit 
    let lookNotHit()=
        Hit.Value.Visible<-false
        NotHit.Value.Visible<-true
    let lookHit()=
        Hit.Value.Visible<-true
        NotHit.Value.Visible<-false
    let resetHit()=
        state<-State.NotHit
        lookNotHit()
    override x._Ready()=
        controlManager.Value.Reset.Add  resetHit
        ()
    
    member x._on_Self_body_entered(body:Node)=
        if state=State.NotHit then
            GD.Print("Target Got Hit")
            state<-State.Hit
            controlManager.Value.targetHit.Trigger()
            lookHit()
            

        ()

