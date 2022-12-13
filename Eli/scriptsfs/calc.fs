
module NewGameProject.Calc
open Godot
open System
let gravitationalConstant =6.6774f* Mathf.Pow(10f,-1f)
let inline gravityForce (m1:float32) (m2:float32) (t1:Vector3 ) (t2:Vector3)=
    //Clamp this becuase we don't have collisions
    let distance2= Math.Max(t1.DistanceSquaredTo(t2),0.01f)
    
    (gravitationalConstant*m1*m2)/(distance2)
let rotateVector90(vec:Vector3)=
        Vector3(vec.z,vec.y,-vec.x)