# Question Responses

## 1. Cloud Service Models

**Question:** Please describe the differences between IAAS, PAAS and SAAS and give examples of each in a cloud platform of your choosing?

**Answer:**

IAAS, PAAS, and SAAS refer to different categories of cloud platform support. Each category provides a certain level of infrastructure, and management responsibility. The range of ownership and responsibility starts with the minimal amount of provided support being IAAS and the maximum amount of support being SASS. If you imagine those as a spectrum with IAAS on the left and SAAS on the right you can imagine one more option to the left of IAAS. This option would be OnPrem(On Premises). With an OnPrem architecture 100% of the infrastructure exists On Premises. Which means the owner of the application has their own servers and databases running at a site that they own and control every aspect of.

Each shift to the right moves some of the physical architecture and / or management responsibility out of the application owners(service user) perview and to a paid cloud service(provider).

_IAAS_(Infrastructure As A Service) provides the least amount of cloud architecture.
In this scenario the Provider provides virtualized physical hardware. Servers, storage, and networking. As well as physical security.
User is responsible for everything inside of the VM: The OS (Linux/Windows), SSH keys, middleware, runtime(.Net sdk, Node.js), and applications. As well as the data.
Examples: Azure Virtual Machines, Amazon EC2

_PAAS_(Platform As A Service) provides additional cloud support.
In this scenario the Provider provides all of the dependencies that are provided with IAAS. In addition to those responsibilities the provider also provides the OS, middleware, and runtime.
User is responsible for application code, database schema and data.
Examples: Azure App Services, Azure Functions, AWS Elastic Beanstalk, AWS Lambda

_SAAS_(Software As A Service) provides the maximum amount of cloud support.
In this scenarion the Provider provides the entire stack. They ensure the application is up and running and bug free.
User is responsible for access management and data. WIth this scenario the SAAS can be configured to take DB snapshots. But this is not always done by default. This ties into the data responsibility. For example the user is responsible for ensuring the validity of their data and their backup strategyh. If you accidently tell the server to delete all of your data it will delete all of your data. If you have not determined and implemented or configured a backup mechanism it is not the responsibility of the cloud service.
Example: Microsoft 365, Amazon Connect, SalesForce

---

## 2. Build vs Buy Decision

**Question:** What are the considerations of a build or buy decision when planning and choosing software?

**Answer:**

### 1. Core Competency

**Is this software what makes our product unique?**

- **Build:** If the software provides a competitive advantage you should own the IP.

- **Buy:** If the software is a "commodity" (e.g., Email, Payroll, or Identity Management), you should buy it.

---

### 2. Total Cost of Ownership (TCO)

This includes ongoing maintenance for the life of the product not just the initial build costs.

- **Build TCO:** Includes developer salaries, QA, DevOps, security patching, bug fixes, and the loss of historical context if the lead engineer leaves. You are assuming a permanent liability.

- **Buy TCO:** Includes licensing fees, integration costs, and potential "Seat" or "Usage" taxes. This is usually a predictable operational expense (OpEx).

---

### 3. Time to Market (TTM)

In a competitive landscape, speed is often more valuable than perfection.

- **Buy:** You can have a SaaS solution configured and integrated in days.

- **Build:** Even with a fast team, a production-ready system requires a full lifecycle (Design $\rightarrow$ Dev $\rightarrow$ Test $\rightarrow$ Deploy). If your competitors are moving faster, building might be a luxury you can't afford.

---

### 4. The "Feature Gap" and Customization

Buy solutions rarely match exactly everything you are looking for.

- **The 80/20 Rule:** If a commercial product meets 80% of your needs, can you adapt your business process to fit the remaining 20%?

- **Vendor Lock-in:** If you buy, you are at the mercy of the vendor's roadmap. If they decide to deprecate a feature you rely on, you have zero recourse.

- **Build:** You have 100% control, but you are responsible for building every feature (like password resets or audit logs) that came with the "Buy" version.

---

### 5. Maintenance and "Technical Debt"

When you build, you are creating a "Legacy" system the moment the code is merged.

- **Maintenance:** Industry standard suggests that 70% of software costs occur after the initial launch. If you build, your team's future capacity is eaten up by maintaining the old code rather than building new features.

- **Buy:** The vendor handles the technical debt. They update the runtimes, fix the security holes, and ensure it works with the latest browsers/OS versions.

---

## 3. Serverless Architecture

**Question:** What are the foundational elements and considerations when developing a serverless architecture?

**Answer:**

A serverless architecture is similar to and often considered a subset of PAAS architecture. With a serverless architecture the focus shifts from managing servers to managing events and state.

### Foundational Elements

Event Driven Design: Serverless Functions are short lived. They are waiting to be triggered. At which point they "wake up". You define the event that triggers the execution. For example the following events could be used to trigger the function.

- HTTP Requests: via API Gateways.

- File Uploads: (e.g., an image landing in an S3 bucket).

- Database Changes: (e.g., a new row in DynamoDB).

- Scheduled Events: (Cron jobs).

Function as a service(FAAS):
This is the logic layer. Functions should be single-purpose. If a function is doing too many things, it becomes harder to scale and debug.

Backend as a service(BAAS):
Since functions are stateless, you rely on external services for everything else:

- Storage: S3 or Google Cloud Storage.

- Databases: NoSQL options like DynamoDB or CosmosDB are preferred for their ability to handle rapid scaling.

- Authentication: Managed services like Auth0 or AWS Cognito.

### Considerations

Stateless: Serverless functions live in isolation. They don't have visibility to data from a previous request. Any data that needs to persist has to be managed seperatley. Either in an external database or cache.

Cold Starts: Serverless functions that have not been called for a while will be spun-down by the cloud service provider. This means that if a function in a spun-down state is called the request will experience a delay while the environment initializes.

Security: With traditional architecture security is generally managed with tools on the network layer. Like firewalls. An authenticated user who has authoriziation to log into the system often has access to several resources. With a serverless architecture each function has it's own permissions. It is managed through Identity Access Management(IAM). So an individual function only has the exact permissions it needs to accomplish it's task. Combining the Single Responsibility Principal with the Principal of Least Privilege allows for greater security. For example a function that reads from a DB only has permission to read. This function does not have permission to edit or delete. So a hacker hacking a read endpoint would not also have access to a write endpoint. In addition to this due to the short life of a serverless function a hacker can not hack an endpoint and then expect maliscious software to persist.

Observability: Serverless architecture is a distributed system. Which makes it less straightforward for traditional logging and debuggin approaches. Distributed tracing tools are available to help follow the path of an event.

Vendor Lock-in: Switching between providers for a serverless system can be non-trivial. Different serverless providers rely on their own specific API's. So switching providers can require significant changes to the application architecture.

---

## 4. Composition Over Inheritance

**Question:** Please describe the concept of composition over inheritance.

**Answer:**

Inheritance is the pattern of building a Parent class and including all of the properties that children of that class might need. Each child of the parent class derives all of their properties and behavior from the parent class. Wether they actually apply to the child class or not. This can lead to undesireable behavior. Like the need to override props and behavior that do not work or apply to the child class. It also makes the Parent/Base class extremely fragile because any change to the parent class could potentially break a child class. It can also lead to confusing deeply nested hierarchies.

Composition is the pattern of building a base class and then 'composing' a more complex object by plugging in smaller components. These smaller components can define different properties and behaviors that apply to different types of child objects. This allows the components to be more loosely coupled, flexible, and testable.

---

## 5. Design Pattern Experience

**Question:** Describe a design pattern you've used in production code. What was the pattern? How did you use it? Given the same problem how would you modify your approach based on your experience?

**Answer:**

[Your answer here]
