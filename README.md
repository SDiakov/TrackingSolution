To provide asynchronous communication between multiple instances of PixelService and a single StorageService it's proposed to use any message/event based approach.

PixelService contains "/track" endpoint action with an abstract message sender for Visit messages injected.
The simple implementation of RabbitMQ producer is set up and bound to this abstraction.

StorageService is implemented as WebService with the ability to register any number of generic hosted services to consume RabbitMQ messages continuously and process them by injected abstract processor.
The hosted service with the file logger as an implementation of such processor for Visit messages is registered.

Several tests are created to cover the crucial parts of services. It was decided to create integration tests with the access to network and file system instead of fully isolated Unit tests:

![image](https://github.com/SDiakov/TrackingSolution/assets/62175094/78492439-b5f6-443e-bf16-0c06d20a83c3)


The services running with docker-compose:

![image](https://github.com/SDiakov/TrackingSolution/assets/62175094/eb8fd60c-8f8b-485d-b258-81566835036f)


The result of calling "/track" endpoint in the browser:

![image](https://github.com/SDiakov/TrackingSolution/assets/62175094/72828e6b-8d06-4a2e-bb92-9c7b2d10bfdd)


The result of calling "/track" endpoint in "/tmp/visits.log" on StorageService:

![image](https://github.com/SDiakov/TrackingSolution/assets/62175094/69370716-2c4b-468d-8f6f-da282e403e05)
