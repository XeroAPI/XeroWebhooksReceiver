# Xero Webhooks Receiver
This sample application demonstrates how to receive webhooks from Xero:

1. Accept a POST request from Xero
2. Verify the payload signature
3. Store the payload to a queue for asynchronous processing
4. Respond with the correct HTTP status code

# Running the application

## Prerequisites
- A private or partner app connected to at least one Xero Organisation, to generate webhook events (https://app.xero.com/Application/)
- A subscription for your app subscribed to contact events (https://developer.xero.com/myapps)
- .NET Core for your preferred platform (https://www.microsoft.com/net/download/core)
- ngrok, to tunnel network traffic to localhost (https://ngrok.com/)

## Running the server
You can choose whether to run this sample application from the packaged executable, or build and run it from the source code. If you're unfamiliar with .NET Core we recommend the executable.

### From the executable
1. Download the packaged executable and unzip to a directory of your choice.
2. Update 'SigningKey.txt' with your Webhook key to hold the key for your subscription. This can be found by browsing to your app at https://developer.xero.com/myapps and selecting the webhooks tab.
3. Open a terminal in the directory and execute `dotnet XeroWebhooksReceiver.dll`. This will start a local web server (e.g. http://localhost:5000).
4. Make your server accessible from Xero as described below.

### From source
1. Clone this repository.
2. Open the solution in a compatible IDE (e.g. Visual Studio Community Edition)
3. Update 'SigningKey.txt' with your Webhook key to hold the key for your subscription. This can be found by browsing to your app at https://developer.xero.com/myapps and selecting the webhooks tab.
4. Build the solution.
5. Run the solution. This will start a local web server (e.g. http://localhost:5000).
6. Make your server accessible from Xero as described below.

## Making your server accessible from Xero
1. Download ngrok from https://ngrok.com/ and unzip to a directory of your choice.
2. Open a terminal in the directory and execute `./ngrok http --bind-tls=true 5000`. Ngrok will provide you with a https URL (e.g. https://daaf38b6.ngrok.io).
3. Set your app's webhook delivery URL to {ngrok_address}**/webhooks** (e.g. https://daaf38b6.ngrok.io/webhooks) in the [developer portal](https://developer.xero.com/myapps/webhooks).
4. Create or modify any contact in a Xero Organisation connected to your app and wait for the event to arrive at your XeroWebhooksReceiver server.

# Solution Structure

Project | Description
--- | ---
`XeroWebhooksReceiver` | Web server and business logic
`WebhooksDataContracts` | Data model classes
`Tests/XeroWebhooksReceiverUnit.Tests` | Unit tests
`Tests/XeroWebhooksReceiverIntegration.Tests` | Integration tests
