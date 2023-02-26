# BudgetBook.Account

## Build the docker image local

```powershell
$version="1.0.0"


$env:GH_OWNER="Brocker591"
$env:GH_PAT="[PAT HERE]"

docker build --secret id=GH_OWNER --secret id=GH_PAT -t "brocker591/budgetbook.account:$version"  .

```

## Run the docker image

```powershell

docker run -it --rm -p 5200:5200 -e MongoDbSettings__Host=mongo -e RabbitMQSettings__Host=rabbitmq --name account --network playinfra_default brocker591/budgetbook.account:$version

```

## Run the docker image with Azure Rescources

```powershell

$cosmosDbConnString="[CONN STRING HERE]"
$serviceBusConnString="[CONN STRING HERE]"

docker run -it --rm -p 5200:5200  -e MongoDbSettings__ConnectionString=$cosmosDbConnString -e ServiceBusSettings__ConnectionString=$serviceBusConnString -e ServiceSettings__MessageBroker="SERVICEBUS" --name account --network playinfra_default brocker591/budgetbook.account:$version

```

## Publishing the Docker image

```powershell

docker push "brocker591/budgetbook.account:$version"
```