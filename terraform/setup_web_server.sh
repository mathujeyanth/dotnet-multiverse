#!/bin/bash
yum update -y
yum install docker -y
service docker start
systemctl enable --now docker
usermod -aG docker ec2-user
docker run -d --restart always -p 80:8080 ghcr.io/mathujeyanth/mj-pwa-audio-converter:latest