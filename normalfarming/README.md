# Normal Farming

This is just how farming works, I'm pretty sure. Like.. because farming sucks? And it also kinda' blows, right? Yes?

Anyway, this game is about farming, and it sucks. But it also blows. It's up to you. I recommend doing both, though.

## TODO

### Basic Shit

- [x] Get some input and movement working

### Sucking + Blowing

- [ ] Add a raycast and detect collisions from center when sucking or blowing
- [ ] Store ref to the thing you're trying to suck/blow; because we latch onto it
- [ ] Make an event/signal from the cone for when sucking or blowing
- [ ] Loose "things" approach when sucked
- [ ] When "things" collide with player, they add themselves to the player's bag
  - [ ] Oh, right... uh... add storage to player struct, I guess?

### Farming

- [ ] Make a 3D tile of a farm plot
  - [ ] I think Godot has like a 3D tile setup I could maybe use?
- [ ] Receive the suck/blow signals from the farm plots
- [ ] Make some kind of visual change for plots that have been... uh... "tilled" let's just say
- [ ] When things are sucked that can change state, spawn their "thing"
- [ ] Add seed crate for sucking up seeds
- [ ] Add pond for sucking up water
- [ ] If a seed is shot into a tilled plot, plot starts growing the thing
- [ ] If you shoot water into a tilled plot, that shit grows FASTER, my dude!!
- [ ] Once a plant is grown, you can suck it up!
- [ ] Shoot grown-ass plants into a thing to sell them.

...

- [ ] ...profit!

### BONUS Shit

- [ ] BONUS: Allow sucking multiple targets -- like an upgrade or something??!?
