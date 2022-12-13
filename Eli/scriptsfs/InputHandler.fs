module NewGameProject.InputHandler
open Godot
open System
type MouseWhel=
|Up
|Down
type InputHandlers={
    massUp:unit->unit
    massDown:unit->unit
    velocityUp:unit->unit
    velocityDown:unit->unit
    placeBody:unit->unit
    play:unit->unit
}
let handleInputs (input:InputHandlers)(event:InputEvent)=
    let mouseWheel=
        if event.IsActionPressed(Actions.velocity_up) then Some MouseWhel.Up
        else if event.IsActionPressed(Actions.velocity_down) then Some MouseWhel.Down
        else None
    if mouseWheel.IsSome then 
        if Input.IsActionPressed("shift") then
           match mouseWheel.Value with
           |Up-> input.massUp()
           |Down-> input.massDown()
        else
            match mouseWheel.Value with
            |Up-> input.velocityUp()
            |Down-> input.velocityDown()
    if event.IsActionPressed("place") then input.placeBody()
    if event.IsActionPressed("play") then input.play()