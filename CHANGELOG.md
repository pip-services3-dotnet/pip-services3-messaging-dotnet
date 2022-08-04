# <img src="https://uploads-ssl.webflow.com/5ea5d3315186cf5ec60c3ee4/5edf1c94ce4c859f2b188094_logo.svg" alt="Pip.Services Logo" width="200"> <br/> Asynchronous messaging for .NET Changelog

## <a name="3.5.0"></a> 3.5.0 (2022-08-04)

### Breaking Changes
* Migrate to .NET 6.0

## <a name="3.4.0"></a> 3.4.0 (2021-09-01)

### Breaking Changes
* Migrate to .NET Core 5.0

## <a name="3.3.4"></a> 3.3.4 (2021-06-11) 
* Revert to version 3.3.0 

## <a name="3.3.0-3.3.3"></a> 3.3.0-3.3.3 (2021-07-05) 
* Adjusted the structure of MessageEnvelope 

### Features
* Updated references as PipServices3.Components have got minor changes

## <a name="3.2.0"></a> 3.2.0 (2021-03-23)

Improved message queues

### Features
* **queues** Added CallbackMessageReceiver to wrap callbacks into IMessageReceiver interface
* **queues** Addded IMessageConnection interface
* **build** Set config params and references to created queues in MessageQueueFactory
* **queues** Added CheckOpen method to MessageQueue
* **queues** Added JSON serialization for MessageEnvelop
* **build** Added IMessageQueueFactory interface
* **build** Added MessageQueueFactory abstract class

## <a name="3.1.1"></a> 3.1.1 (2020-06-26)

### Features
* Implemented support backward compatibility

## <a name="3.1.0"></a> 3.1.0 (2020-05-26)

### Breaking Changes
* Migrated to .NET Core 3.1

## <a name="3.0.0-3.0.3"></a> 3.0.0-3.0.3 (2020-01-13)

### Breaking Changes
* Moved to a separate package
* Added 'pip-services' descriptors

## <a name="2.3.4"></a> 2.3.4 (2018-06-11)
* **status** Added StatusRestService
* **status** Added HeartbeatRestService
* **rest** Added ResolveAllAsync method to HttpConnectionResolver

## <a name="2.2.1"></a> 2.2.1 (2018-03-19)
* **rest** Added base routes
* **rest** Added retries to RestClient
* Reimplemented HttpEndpoint
* **rest** HttpResponseSender
* **rest** HttpConnectionResolver

## <a name="2.1.0-2.1.6"></a> 2.1.0-2.1.6 (2018-03-09)
* **rest** Add HttpEndpoint
* **rest** Add exception handling in RestService

### Features
* Converted to .NET Standard 2.0

## <a name="2.0.0-2.0.8"></a> 2.0.0-2.0.8 (2017-06-12)

### Features
* **rest** Add missing functionality of DirectClient
* **rest** Add CommandableHttpClient and CommandableHttpService
* **test** Re-structure tests - separate pure unit-tests libraries

## <a name="2.0.0"></a> 2.0.0 (2017-02-25)

### Breaking Changes
* Migrated to pip-services3-commons 2.0

## <a name="1.0.0"></a> 1.0.0 (2016-11-21)

Initial public release

### Features
* **messaging** Abstract and in-memory message queues
* **rest** RESTful service and client
* **rest** Implemented connection parameters and credentials
* **messaging** MsmqMessageQueue

### Bug Fixes
No fixes in this version

