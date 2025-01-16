![UnityVersion](https://img.shields.io/static/v1?label=unity&message=2022.1%2B&color=blue&style=flat&logo=Unity)
![GitHub last commit](https://img.shields.io/github/last-commit/dotmos/gameframework2)
![GitHub](https://img.shields.io/github/license/dotmos/gameframework2)
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
