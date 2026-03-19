# CLDV7311-POE

CLDV7311 Part 1 P.O.E
ST10296771 Mmangaliso Malunga

# Section 1:
<img width="940" height="805" alt="image" src="https://github.com/user-attachments/assets/be2b9ba5-4841-4dbf-a3ec-a5629d4278ca" />


# Section 2: 
Web App: 
GitHub: https://github.com/M-Malunga/CLDV7311-POE.git 

# Section 3:
Question 1:
The difference between cloud deployments and on-premise deployments mostly lie in how you shift from managing everything physically to managing them externally or by using a service.
Security:
When it comes to security, we can define it as follows: on-premise defines that you are fully in charge of each and every single attribute and aspect that goes into protecting your data. This is both an advantage and a burden, as when it comes to physical data centre security, you have to make sure that no external individual can come in of their own accord. You are directly in charge of servers, networks, and applications. You must be able to procure firewalls, maintain them, have intruder alert systems, and patch the security for an entire stack. This means you are highly related to high isolation and deep, demanding specialization.

As for cloud, you are fully sharing the responsibility of your system or application in this regard. As an example, utilizing the Azure cloud provider, they would become directly responsible for the security aspect, which could be physical infrastructure, networks, and the hypervisor. I, as the user who created the application or system, am directly in charge of security when it comes to what is in the cloud-specifically my data and how it can be utilized, configured, and accessed.

An example would be a team hosting a Windows application, meaning they would have to install and configure web servers like IIS or Apache and then proceed to set up a firewall. Whereas, as shown in Chapter 1 of the Developer's Guide, if you deploy a similar type of application using the Azure App Service (PaaS), Azure will manage the operating system and web server patches for you. Then, all you would have to do is focus on higher-level security, as an example utilizing Azure Active Directory for authentication, which within the guide is Figure 1.37. Another example is utilizing network-level tools like Network Security Groups (NSGs), as mentioned in Cloud Computing Technology on page 47.

Deployment Speed and Agility:
In regard to on-premise deployments, it is usually slow and very intensive when it comes to the capital of a process. It usually focuses around getting hardware, being stopped by deliveries, and having to set up servers, cables, and operating systems. Then, eventually, you get to the point of deploying the application. This could usually take between weeks and months only when you are utilizing a physical system like servers and towers.

As for cloud deployments, it is usually fast and allows for the user in question to do it themselves without having to encounter assistance, meaning it is self-service. With this, it is possible for resources to be available in the moments, often within minutes, through a portal, a client, or an API. This allows for agile development, continuous delivery, and speed.

An example is when deploying a web application on-premise, you might have to wait a few months only for a new server. Whereas in Azure, as shown in Chapter 1 of Azure for Developers, it is entirely possible to create a web application resource and administer the portal or Visual Studio, as necessitated for this assessment-all can be seen in Figure 1.18 within the guide. This allows for quick spin-up of testing environments and rolling back to previous versions, meaning you can always go back to a time before you had encountered a problem, if it was saved on the cloud. Going back allows for you to be able to quickly apply anything to the current need.

Resource Management and Scalability:
When it comes to on-premise resource management, it becomes rigid and it is very hard to plan around capacity for anything, especially hardware-related. You have to have the ability to predict hardware prices and how the peak load of your hardware might lead to you over-providing or under-providing, wasting resources, and having diminished performance, especially during “peak hours.” As for scaling, it requires the purchasing of more hardware.

As for the cloud, resources are more undefined, abstract, and elastic. It is entirely capable or possible for you to scale up, which means increasing the power of a single resource, or scale out, increasing the instances and the number thereof dynamically or automatically to match any growing demand and changes within your application that you need to meet. You only pay for what you utilize.

An example is where we consider EventEase as a fully functional application. During a major event at places like stadiums, it is entirely possible for the traffic on that system to spike exponentially. For an on-premise situation, it is possible for the web app to crash. In the cloud, as discussed in Chapter 9 of Azure for Developers, you can set the web app services to scale automatically in case of any sudden changes. If I'm not mistaken, this automatically happens once the CPU usage has exceeded 80% or 70%. Continuing with our example, that means that when the traffic subsides, the system will automatically scale everything down, which would allow you to save money on the cloud system. This is a more obvious showcasing of how physical versus abstract consumption of resources becomes more evident.

Question 2:

Infrastructure as a Service:
This is where access is provided, should it be requested, to any IT infrastructure-as an example, servers, virtual machines, and networks. As the user, you are entirely responsible for the management of software. It is like buying a car and making sure you don't crash it, or buying a house and then having to furnish it. As an example, you could buy a virtual machine for a custom database, and in there, you have to configure and set up the software.

Platform as a Service:
Platform as a Service provides a managed platform for developing and application management. Here, you would only focus on providing code, while the provider would handle the operating system and its infrastructure. For this example, it's like buying a house, but you do not have to pay for or deal with its maintenance. An example here is also when you deploy .NET code to Azure; you do not specifically manage the operating system or the server.

Software as a Service:
This is where you are provided with a ready-made system or software solution, allowing you to simply use the application, but you have no say in anything related towards its back end. One example would be Google Workspace. Another practical example is living in a rented room for a day.

EventEase:
When it comes to EventEase and how they would utilize and benefit from PaaS, it allows for a new system, custom-designed for them, to be created and controlled. It is there for convenience, but to break it down:

Focusing on the Business Logic: This means that the core value of EventEase is event management features. They can develop and focus 100% on writing the code that creates, sells, and manages events, while not managing the database and web server, as all of this would be done automatically.

Highly Scalable: Considering that EventEase is a platform with little necessity for resources initially, there is a higher possibility of everything being scaled up as it has to grow. Platform as a Service is designed to handle this from the ground up. An example is that EventEase can be configured to auto-scale automatically based on traffic by utilizing cloud features such as Azure Traffic Manager. This means that one regional server will not have to manage the flow of traffic for the entire system for all global events, providing a more reliable experience for users.

Developer-Friendly Productivity: It is built for more modern practices and integrates highly with Azure CI and CD pipelines. An example is that a developer can set up continuous deployments from GitHub directly to the Azure App Service, as done within this assessment. This means that any time code is pushed to the main branch on GitHub, it will automatically build and remove the necessity for downtime.

In Conclusion:
I aim to say that with Infrastructure as a Service, EventEase would have had to maintain all of the aforementioned aspects, such as scaling and management of virtual machine infrastructure. This is like buying individual sections of a car and building it yourself; it is not time-conservative. With Software as a Service, they would only have a common and unique prebuilt events management platform, which loses the unique flair that could have been possible with a custom application. PaaS provides the perfect balance of freedom and flexibility while allowing simplicity.
