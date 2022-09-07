# GameFramework2

Requires Unity 2022.1 or later

# Getting Started

Clone the repo and open it with Unity.
Press play.

Have a look at ExampleCore and ExampleGamestate to get an idea of how things work.

# TODO

Implement missing functions for non-Unity projects

Services:
- Service dependecies during initialization?
- Create a service for enabling/disabling a loading screen as well as updating the loadingscreen progress

Gamestates:
- GamestateService.Switch is not threadsafe. That should not be a problem though, just switch from main thread?
- Create a list of followup gamestates that should be loaded when the current gamestate is finished?

Generator:
- Components: Abstract serialization so the implementation works with different serializers
- Services: Automagically create "serviceData" and implement serialization for it
