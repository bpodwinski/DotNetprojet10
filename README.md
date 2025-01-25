Ce projet vise à gérer les patients par un organisateur, permettre la prise de notes par un praticien, et évaluer le risque de diabète chez les patients.

D'un point de vue technique, l'objectif est de développer une application reposant sur une architecture en microservices, en intégrant une base de données NoSQL pour la gestion des notes.

Diagramme représentant l'architecture de l'application :
```mermaid
	graph TD;
	Ocelot[Ocelot Gateway]  -->  AuthService[Backend: Auth API]
	Ocelot[Ocelot Gateway]  -->  PatientService[Backend: Patient API]
	Ocelot  -->  ReportService[Backend: Report API]
	Ocelot  -->  NoteService[Backend: Note API]
	AuthService  -->  DbAuth[(SQL Server)]
	PatientService  -->  DbPatient[(SQL Server)]
	NoteService  -->  DbNote[(MongoDB)]
	ReportService  <-->  Elasticsearch[Engine: Elasticsearch]
	Client[Frontend: Blazor Web]  <-->  Ocelot
```
## Prérequis
Docker : https://www.docker.com/

## Installation
Installer Docker.

1. Cloner le projet 
	```
	git clone https://github.com/bpodwinski/DotNetprojet10.git
	```
2. Dans le répertoire du projet utiliser docker compose pour créer les images et lancer les conteneurs docker :
	```
	docker compose up
	```
