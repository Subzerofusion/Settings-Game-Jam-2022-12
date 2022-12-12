module NewGameProject.GUI
open System
open RayCast
open Orbitals.Nodes
open Godot
open Calc
open Godot.Collections
open Control
type GUIHandler() as this=
    inherit CanvasLayer()
    let controlManager=lazy(this.GetParent<ControlManager>())
    let stats=lazy(this.GetNode<Label>("Stats"))
    let mouseSateLabel=lazy(this.GetNode<Label>("MouseState"))
    let showStats(state)=
        GD.Print "showing stats"
        let stats= stats.Value
        
        stats.Text<-
            $"""
            Velocity={state.VelocityMagnitude}
            Mass={state.Mass}
            """
        stats.Show()
    let showMouseState mouseState=
        let mouseStateText=
            match mouseState with
            |Selecting -> "selecting"
            | PlacingObject(_, _) -> "Placing Orbiter"
            | Deleting -> "Deleting"
        mouseSateLabel.Value.Text<-mouseStateText
    override x._Ready()=
        controlManager.Value.PlacementState.Event.Add showStats
        controlManager.Value.MouseState.Event.Add showMouseState
        ()

