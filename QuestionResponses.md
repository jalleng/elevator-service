# Question Responses

## 1. Cloud Service Models

**Question:** Please describe the differences between IAAS, PAAS and SAAS and give examples of each in a cloud platform of your choosing?

IAAS, PAAS, and SAAS refer to different categories of cloud platform support. Each category provides a certain level of infrastructure and management responsibility. The range of ownership and responsibility starts with the minimal amount of provided support being IAAS and the maximum amount of support being SAAS. If you imagine those as a spectrum with IAAS on the left and SAAS on the right, you can imagine one more option to the left of IAAS: **OnPrem** (On Premises). With an OnPrem architecture, 100% of the infrastructure exists on premises, meaning the owner of the application has their own servers and databases running at a site that they own and control every aspect of.

Each shift to the right moves some of the physical architecture and/or management responsibility out of the application owner's (service user) purview and to a paid cloud service (provider).

### IAAS (Infrastructure As A Service)

Provides the least amount of cloud architecture.

- **Provider responsibilities:** Virtualized physical hardware (servers, storage, networking) and physical security
- **User responsibilities:** Everything inside the VM - OS (Linux/Windows), SSH keys, middleware, runtime (.NET SDK, Node.js), applications, and data
- **Examples:** Azure Virtual Machines, Amazon EC2

### PAAS (Platform As A Service)

Provides additional cloud support.

- **Provider responsibilities:** All IAAS dependencies plus OS, middleware, and runtime
- **User responsibilities:** Application code, database schema, and data
- **Examples:** Azure App Services, Azure Functions, AWS Elastic Beanstalk, AWS Lambda

### SAAS (Software As A Service)

Provides the maximum amount of cloud support.

- **Provider responsibilities:** The entire stack - ensures the application is up, running, and bug-free
- **User responsibilities:** Access management and data. The user is responsible for ensuring data validity and backup strategy. For example, if you accidentally delete all your data, it will be deleted - if you haven't configured backups, the cloud service is not responsible
- **Examples:** Microsoft 365, Amazon Connect, Salesforce

---

## 2. Build vs Buy Decision

**Question:** What are the considerations of a build or buy decision when planning and choosing software?

### 1. Core Competency

Is this software what makes our product unique?

- **Build:** If the software provides a competitive advantage, you should own the IP
- **Buy:** If the software is a "commodity" (e.g., Email, Payroll, or Identity Management), you should buy it

### 2. Total Cost of Ownership (TCO)

This includes ongoing maintenance for the life of the product, not just the initial build costs.

- **Build TCO:** Includes developer salaries, QA, DevOps, security patching, bug fixes, and the loss of historical context if the lead engineer leaves. You are assuming a permanent liability
- **Buy TCO:** Includes licensing fees, integration costs, and potential "seat" or "usage" fees. This is usually a predictable operational expense (OpEx)

### 3. Time to Market (TTM)

In a competitive landscape, speed is often more valuable than perfection.

- **Buy:** You can have a SaaS solution configured and integrated in days
- **Build:** Even with a fast team, a production-ready system requires a full lifecycle (Design → Dev → Test → Deploy). If your competitors are moving faster, building might be a luxury you can't afford

### 4. The "Feature Gap" and Customization

Buy solutions rarely match everything you are looking for.

- **The 80/20 Rule:** If a commercial product meets 80% of your needs, can you adapt your business process to fit the remaining 20%?
- **Vendor Lock-in:** If you buy, you are at the mercy of the vendor's roadmap. If they decide to deprecate a feature you rely on, you have zero recourse
- **Build:** You have 100% control, but you are responsible for building every feature (like password resets or audit logs) that came with the "buy" version

### 5. Maintenance and "Technical Debt"

When you build, you are creating a "legacy" system the moment the code is merged.

- **Maintenance:** Industry standard suggests that the majority of software costs occur after the initial launch. If you build, your team's future capacity is eaten up by maintaining old code rather than building new features
- **Buy:** The vendor handles the technical debt. They update runtimes, fix security holes, and ensure compatibility with the latest browsers/OS versions

---

## 3. Serverless Architecture

**Question:** What are the foundational elements and considerations when developing a serverless architecture?

A serverless architecture is similar to and often considered a subset of PAAS architecture. With a serverless architecture, the focus shifts from managing servers to managing events and state.

### Foundational Elements

#### 1. Event-Driven Design

Serverless functions are short-lived. They are waiting to be triggered, at which point they "wake up". You define the event that triggers execution. For example:

- **HTTP Requests:** via API Gateways
- **File Uploads:** (e.g., an image landing in an S3 bucket)
- **Database Changes:** (e.g., a new row in DynamoDB)
- **Scheduled Events:** (Cron jobs)

#### 2. Function as a Service (FAAS)

This is the logic layer. Functions should be single-purpose. If a function is doing too many things, it becomes harder to scale and debug.

#### 3. Backend as a Service (BAAS)

Since functions are stateless, you rely on external services for everything else:

- **Storage:** S3 or Google Cloud Storage
- **Databases:** NoSQL options like DynamoDB or CosmosDB are preferred for their ability to handle rapid scaling
- **Authentication:** Managed services like Auth0 or AWS Cognito

### Considerations

#### 1. Stateless

Serverless functions live in isolation. They don't have visibility to data from a previous request. Any data that needs to persist must be managed separately, either in an external database or cache.

#### 2. Cold Starts

Serverless functions that haven't been called for a while will be spun down by the cloud service provider. This means that if a function in a spun-down state is called, the request will experience a delay while the environment initializes.

#### 3. Security

With traditional architecture, security is generally managed with tools on the network layer, like firewalls. An authenticated user who has authorization to log into the system often has access to several resources.

With a serverless architecture, each function has its own permissions managed through Identity Access Management (IAM). An individual function only has the exact permissions it needs to accomplish its task. Combining the Single Responsibility Principle with the Principle of Least Privilege allows for greater security. For example, a function that reads from a database only has permission to read - not edit or delete. So a hacker compromising a read endpoint would not also have access to a write endpoint. Additionally, due to the short life of a serverless function, malicious software cannot persist.

#### 4. Observability

Serverless architecture is a distributed system, which makes it less straightforward for traditional logging and debugging approaches. Distributed tracing tools are available to help follow the path of an event.

#### 5. Vendor Lock-in

Switching between providers for a serverless system can be non-trivial. Different serverless providers rely on their own specific APIs, so switching providers can require significant changes to the application architecture.

---

## 4. Composition Over Inheritance

**Question:** Please describe the concept of composition over inheritance.

**Inheritance** is the pattern of building a parent class that includes all of the properties that children of that class might need. Each child of the parent class derives all of their properties and behavior from the parent class, whether they actually apply to the child class or not. This can lead to undesirable behavior, such as:

- The need to override properties and behavior that don't work or apply to the child class
- An extremely fragile parent/base class, because any change could potentially break a child class
- Confusing, deeply nested hierarchies

**Composition** is the pattern of building a base class and then "composing" a more complex object by plugging in smaller components. These smaller components can define different properties and behaviors that apply to different types of child objects. This allows the components to be more loosely coupled, flexible, and testable.

---

## 5. Design Pattern Experience

**Question:** Describe a design pattern you've used in production code. What was the pattern? How did you use it? Given the same problem how would you modify your approach based on your experience?

I was building a "bridge" between a legacy system and a new system. The goal of this bridge was to keep data moving from the legacy system (the source of truth) to the new system. Data also needed to flow from the new system back to the legacy system to update the source of truth. This was part of a microservice architecture.

### Data Flow: New System → Legacy System

Due to security and functional constraints, the data needed to follow a fire-and-forget paradigm - the new system didn't need confirmation that data reached the legacy system.

I built this using a backend that received HTTP requests and routed traffic to the appropriate microservice. The microservices were identical, but there were multiple instances because each microservice was connected to a specific region/database for the customer.

**In hindsight:** I decided that for this behavior, a Kafka queuing approach would have been a better option than an API receiving HTTP requests. It would have simplified the architecture and made it easier to maintain.

### Data Flow: Legacy System → New System

I needed data available in the microservice that would be updated from the legacy side and readable from the new side. This was data that would populate options in a UI in the new system.

**Constraints:**

- It needed to be accurate and up to date
- It needed to be accessible quickly and repeatedly (think of a person opening and closing a dropdown)
- It could not directly query the legacy system (to avoid putting load on it)
- It needed to be filterable by several parameters

I chose to use a **Redis caching approach** because it fit well with the constraints.

**Lessons learned:** Later, after implementing the caching approach, the customer decided to go in a slightly different direction which necessitated having more information in the microservice. As complexity grew and we implemented Postgres to manage larger and more numerous data records, it became obvious that a relational approach would have worked better overall than the initial Redis implementation.
