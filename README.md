# GameFramework2

TODO:

Services:
- Service dependecies during initialization?
- Create a service for enabling/disabling a loading screen as well as updating the loadingscreen progress

Is the Thread Rendevouz happening on MainThread?

GamestateService.Switch is not threadsafe. That should not be a problem though, just switch from main thread?

Gamestates:
- Create a list of followup gamestates that should be loaded when the current gamestate is finished?

Generator:
- Components: Abstract serialization
- Services: Automagically create "serviceData" and implement serialization for it
