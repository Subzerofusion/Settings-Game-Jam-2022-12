namespace ScriptsFs

open Godot

type SimplePrintFs() =
    inherit Node()

    override this._Ready() =
        GD.Print("SimplePrintFs: F# Running...")