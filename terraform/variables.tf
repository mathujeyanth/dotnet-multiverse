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

variable "cloudflare_ipv4" {
  description = "Cloudflare IPv4 ranges"
  type        = list(string)
  default = [
    "173.245.48.0/20",
    "103.21.244.0/22",
    "103.22.200.0/22",
    "103.31.4.0/22",
    "141.101.64.0/18",
    "108.162.192.0/18",
    "190.93.240.0/20",
    "188.114.96.0/20",
    "197.234.240.0/22",
    "198.41.128.0/17",
    "162.158.0.0/15",
    "104.16.0.0/13",
    "104.24.0.0/14",
    "172.64.0.0/13",
    "131.0.72.0/22"
  ]
}

variable "cloudflare_ipv6" {
  description = "Cloudflare IPv6 ranges"
  type        = list(string)
  default = [
    "2400:cb00::/32",
    "2606:4700::/32",
    "2803:f800::/32",
    "2405:b500::/32",
    "2405:8100::/32",
    "2a06:98c0::/29",
    "2c0f:f248::/32"
  ]
}

variable "ssh_allowed_cidrs" {
  description = "List of CIDRs allowed to ssh into EC2"
  type        = list(string)
}