module NewGameProject.OrbitalPhysics
open Godot
open System
[<Measure>]type kg
[<Measure>]type m

[<Struct>]
type OrbitalData={
    Mass:float32
}

[<Struct>]
type OrbitalBody=
    {
        Velocity:Vector3
        Transform:Vector3
        OrbitalData:OrbitalData
        ID:uint64
        acceleration:Vector3
    }

    
//TODO: check for collisions and if so, stop the simulation  
let calcAcceleration  entites this=
    entites|>Array.sumBy(fun other->
        if this.ID=other.ID then Vector3.Zero
        else
            let direction=this.Transform.DirectionTo(other.Transform)
            if direction.x=0f && direction.y=0f && direction.z=0f then GD.PrintErr "direction had magnitude zero while calculating physics. this means two objects are identical which is very bad"
            let force=direction* Calc.gravityForce this.OrbitalData.Mass other.OrbitalData.Mass this.Transform other.Transform
            //TODO this is unneccissay
            let acceleration= force/this.OrbitalData.Mass
            acceleration
    )

//https://en.wikipedia.org/wiki/Verlet_integration
let nextState (dtS:float32) (entites:OrbitalBody array)= 
        (entites)|>Array.map(fun this->
        let pos=this.Transform
        let vel=this.Velocity
//        GD.Print(sprintf"  pos= %A"  pos) 
//        GD.Print(sprintf" velocity= %A"  vel)
        let newAcc= calcAcceleration entites this
        //TODO: if i calculate my acceleration here it could reduce the number of iterations. Though it will only work if i am doing a copy+update instead of mutating the original 
        let newPos = pos + vel*dtS + this.acceleration*(dtS*dtS*0.5f); // only needed if acceleration is not constant
        let newVel = vel + (this.acceleration+newAcc)*(dtS*0.5f)
//        GD.Print(sprintf"  new pos= %A"  newPos) 
//        GD.Print(sprintf" new velocity= %A"  newVel) 
        //TODO: this is very expensive in memory and may not be worth while
        {this with Velocity=newVel;Transform=newPos;acceleration=newAcc}
        )
    
//This isn't really working
let calculateAccelerations (entites:OrbitalBody array)=
    entites|>Array.map (calcAcceleration entites)

let stepTime (dtMs:int) (entites:OrbitalBody array)=
    let dtS=float32 dtMs/1000f
//    let newAccs= calculateAccelerations(entites)
    let next=nextState dtS entites
    next

let accumulateSteps entites dtMs steps=
    let states=Array.zeroCreate steps
    GD.Print $"calculating physics for {steps} steps"
    states[0]<-entites
    for i=1 to steps-1 do 
        states[i]<-stepTime dtMs states[i-1]
    states

    
type OrbitManager()=
    let orbitEntities:OrbitalBody array=Array.empty
