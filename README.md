# Auto-scaling in AKS using KEDA

This documentation on how to enable KEDA, deploy a sample .NET 6 application in AKS, and auto-scale using KEDA. Below are the necessary prerequisites, overview of the sample projects, assumptions, and steps to follow.

## Prerequisites

Make sure you have the following tools installed:

- Docker
- Azure CLI
- kubectl
- Helm

You also need:

- An Azure subscription
- A Container registry
- A Kubernetes service that has access to the above container registry

## Overview of Sample Projects

The sample projects can be found in the repository repo-url and consist of the following 3 projects:

1. **Consumer:** This project consumes messages from a queue.
2. **Producer:** This project pushes messages to a queue.
3. **Contracts:** This project contains common DTOs.

- **Queue provider** used in this setup is **RabbitMQ**.
- **Consumer** and **RabbitMQ** will be deployed to the AKS cluster.
- **Consumer** will be auto-scaled based on the length of the specified queue.
- **Publisher** can be run on a local machine.

## Assumptions

Before proceeding, ensure the following assumptions are valid:

- Azure Resource Group Name: **ResGrp**
- Azure Kubernetes Service Name: **aks_cluster**
- Namespace created: **keda-test**
- Azure Container Registry Name: **acrtest**
- RabbitMQ is publicly accessible at IP: **20.241.242.61**

## Setup

1. Login with Azure CLI using PowerShell or Command Prompt:
    ```
    az login
    ```

2. Get credentials for the AKS cluster:
    ```
    az aks get-credentials --resource-group ResGrp --name aks_cluster
    ```

3. Setup RabbitMQ in AKS:
    ```
    kubectl create namespace keda-test
    helm repo add bitnami https://charts.bitnami.com/bitnami
    helm install rabbitmq --set auth.username=user,auth.password=Pass123,service.type=LoadBalancer bitnami/rabbitmq -n keda-test
    ```

4. RabbitMQ will be accessible through a random IP allocated by AKS. Assume 20.241.242.61.
   Create a queue named `message.queue` in RabbitMQ using the management UI available at http://20.241.242.61:15672.
   Login with Username: user, Password: Pass123.

5. Enable KEDA:
   - Install AKS-Preview Extension:
     ```
     az extension add --name aks-preview
     az extension update --name aks-preview
     ```
   - Register AKS-Keda Preview:
     ```
     az provider register --namespace Microsoft.ContainerService
     az feature register --namespace "Microsoft.ContainerService" --name "AKS-KedaPreview"
     az feature show --namespace "Microsoft.ContainerService" --name "AKS-KedaPreview"
     ```
   - Enable KEDA for the AKS cluster:
     ```
     az aks update --resource-group ResGrp --name aks_cluster --enable-keda
     ```

6. Build and Push the image to the Container Registry:
   ```
   az acr login --name=acrtest
   cd /path/to/consumer/project
   az acr build --image consumer:latest --registry acrtest .
   ```

7. Deploy Consumer application to AKS:
   ```
   kubectl apply -f consumer.deploy.yaml -n keda-test
   ```

## ScaledObject Specification

The `consumer.deploy.yaml` file contains the ScaledObject specification used for auto-scaling the Consumer application based on RabbitMQ queue length.

```yaml
apiVersion: keda.k8s.io/v1alpha1
kind: ScaledObject
metadata:
  name: consumer-scaler
  namespace: keda-test
spec:
  scaleTargetRef:
    name: message-consumer
  pollingInterval: 5
  cooldownPeriod: 10
  minReplicaCount: 0
  maxReplicaCount: 20
  triggers:
    - type: rabbitmq
      metadata:
        queueName: message.queue
        host: 'amqp://user:Pass123@rabbitmq.keda-test.svc.cluster.local:5672'
        queueLength: '5'
        rabbitmqExchange: ""
        rabbitmqExchangeType: ""
        rabbitmqRoutingKey: ""
        rabbitmqPrefetchCount: "1"
```

This specification utilizes RabbitMQ as the scaler. After every 5 seconds (pollingInterval), it checks whether the length of the queue (`message.queue`) equals 5. If true, it triggers auto-scaling and consumes more messages from the queue concurrently.

## References

- https://keda.sh/
- https://learn.microsoft.com/en-us/azure/aks/keda-deploy-add-on-cli
- https://www.gokhan-gokalp.com/en/kubernetes-based-event-driven-autoscaling-with-keda-rabbitmq-and-net-core/