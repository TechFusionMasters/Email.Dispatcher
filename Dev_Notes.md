## Stack
- RabbitMQ
- .NET Worker Service
- Database (SQL Server)
- SMTP / Email Provider

## .Net Technical terms used :
- WEB API
- Entity Framework
- Minimal Api
- Global exception handling and developer exception page
- DB First approach
- Repository pattern and DI
- Asynchronous Initialization via Hosted Service

## RabbitMQ docker setup command
# latest RabbitMQ 4.x
- docker run -it --rm --name rabbitmq -p 5672:5672 -p 15672:15672 rabbitmq:4-management

## RabbitMQ client
- Should have single connection for whole application(singleton)
- Can create multiple channel as required

## Connection: Physical link to broker

- Channel: Lightweight AMQP session

- Exchange: Routes messages

- Routing key: Address label

- Queue: Stores messages

- Binding: Routing rule

- Consumer: Processes messages


## 📨 RabbitMQ Publish Flow
    ## 1️⃣ Connection

      CreateConnectionAsync() opens a TCP connection

      Heavy operation

      Should be reused, not created per message

      Thread-safe

    ## 2️⃣ Channel

      CreateChannelAsync() creates an AMQP channel

      Lightweight

      Used for publish/consume operations

      Multiple channels can share one connection

    ## 3️⃣ Queue Declaration

      QueueDeclareAsync() ensures queue exists

      Idempotent (safe to call multiple times)

      Fails if queue exists with different settings

      Flags

      durable → survives broker restart

      exclusive → only one connection can use it

      autoDelete → deleted when last consumer disconnects

    ## 4️⃣ Message Serialization

      Messages are sent as byte[]

      Common format: JSON → UTF-8 bytes

    ## 5️⃣ Publishing Message

      BasicPublishAsync() sends message to exchange

      Default exchange ("") routes by queue name

      Publish ≠ delivered

      Publish ≠ persisted