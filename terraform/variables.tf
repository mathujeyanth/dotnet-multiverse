variable "domain" {
  description = "Domain name"
  type        = string
}

variable "profile" {
  description = "aws profile"
  type        = string
  default     = ""
}

variable "docker_image" {
  description = "Docker image with digest"
  type        = string
}

variable "ec2_private_key" {
  description = "Private key for EC2 SSH access"
  type        = string
  sensitive   = true
}