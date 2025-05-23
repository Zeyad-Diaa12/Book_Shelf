#!/bin/bash

# Replace "yourusername" with your actual Docker Hub username
DOCKER_HUB_USERNAME="zeyaddiaa500"

# Tag the BookShelf application images
docker tag project-bookshelf-app $DOCKER_HUB_USERNAME/bookshelf-app:latest
docker tag postgres $DOCKER_HUB_USERNAME/bookshelf-db:latest

# Push the images to Docker Hub
docker push $DOCKER_HUB_USERNAME/bookshelf-app:latest
docker push $DOCKER_HUB_USERNAME/bookshelf-db:latest

echo "Successfully pushed BookShelf project to Docker Hub!"

