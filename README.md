# GlueHome

## Setup

### Local Development

To setup an instance of the required dependencies for local development run the following

```
docker compose --profile local-development up
```

This will setup an instance of the following services

- MongoDB @ localhost:27017

### Running Complete Setup

A copy of the API with all supporting services can be run with the following

```
docker compose --profile all up --build
```

This will setup an instance of the following services

- Glue API @ localhost:8080
- MongoDB @ localhost:27017
- Jaeger Web UI @ localhost:16686
- Grafana @ localhost:3000

## Assumptions

- A delivery can only be created by a partner
- Only 1 delivery can exist per order id

## Swagger

The swagger UI can be accessed at <http://localhost:8080/swagger> when running everything through `docker compose`

## Authentication

For the purposes of keeping this simple this currently uses basic authentication with two hardcoded users, see improvements section for more information on this.

The hardcoded users are:

- `user:user`
- `partner:partner`

## OpenTelemetry

This makes use of OpenTelemetry to collect logging, metrics, and tracing to aid in monitoring of the service once deployed into a cloud environment.

When running with `docker compose` it will automatically configure the OpenTelemetry collection which can then be viewed at the following locations

- Traces:
    - Jaeger: <http://localhost:16686/search>
    - Grafana: [http://localhost:3000/explore](http://localhost:3000/explore?orgId=1&left=%7B%22datasource%22:%22jaeger%22,%22queries%22:%5B%7B%22refId%22:%22A%22,%22datasource%22:%7B%22type%22:%22jaeger%22,%22uid%22:%22jaeger%22%7D,%22queryType%22:%22search%22,%22service%22:%22Glue-API%22%7D%5D,%22range%22:%7B%22from%22:%22now-1h%22,%22to%22:%22now%22%7D%7D)

- Metrics:
    - Grafana: [http://localhost:3000/explore](http://localhost:3000/explore?orgId=1&left=%7B%22datasource%22:%22prom%22,%22queries%22:%5B%7B%22refId%22:%22A%22,%22datasource%22:%7B%22type%22:%22prometheus%22,%22uid%22:%22prom%22%7D%7D%5D,%22range%22:%7B%22from%22:%22now-1h%22,%22to%22:%22now%22%7D%7D)

- Logs:
    - Grafana: [http://localhost:3000/explore](http://localhost:3000/explore?orgId=1&left=%7B%22datasource%22:%22loki%22,%22queries%22:%5B%7B%22refId%22:%22A%22,%22expr%22:%22%7Bexporter%3D%5C%22OTLP%5C%22%7D%20%7C%3D%20%60%60%22,%22queryType%22:%22range%22,%22datasource%22:%7B%22type%22:%22loki%22,%22uid%22:%22loki%22%7D,%22editorMode%22:%22builder%22%7D%5D,%22range%22:%7B%22from%22:%22now-1h%22,%22to%22:%22now%22%7D%7D)

## Improvements

- Better Authentication

    Authentication could be handled by a trusted service that issues signed tokens (JWT) that include the users identity and permissions, this would allow of fine grained control over the API endpoints and allow filtering of the response to only the data that belongs to the user

- Testing

    There needs to be an expansion of the unit tests to include more scenarios, but there also needs to be a suite of integration tests that will execute the code against real instances of the database and other external services to ensure correct behavior

- Request Validation

    There should be improvements to the validation of requests to ensure they are valid and provide better feedback on problems

- Data Storage

    This is currently using a very simple schema for data storage that is in no way optimized for a larger API.

    There also needs to be careful management of some of the data due to it container PII, this should be segregated for security/privacy and stored in a way that meets requirements of best practices and local laws

- Logging

    There should be significantly more logging and tracing through out the code to help with root cause analysis once this is deployed into a cloud environment

    Again because of the presence of PII these logs need to be careful to not expose this information

- Deployment

    Assuming a deployment to Kubernetes there should be a helm chart created that defines all the deployment needs of this service

- Caching

    To improve response times and reduce load on the data store a read through cache should be implemented

- Pub/Sub

    In order to provide partners with updates about the state of deliveries a webhook mechanism could be added, although this would be better as an additional micro service to ensure that any issues created by calling third party endpoints did not impact the performance and reliability of the main API

    This pub/sub mechanism could be implemented in a couple of ways:
    
    - This API raised an event on API calls that change the state, this event is then pushed into a message queue (something like Kafka) that is then processed by another service that makes the call to any registered webhooks for the relevant users
    - There is another service that watches a change stream from the data store (assuming support) that then makes the calls to the relevant webhooks