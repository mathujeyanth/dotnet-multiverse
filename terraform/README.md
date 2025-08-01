# Terraform

## Commands
`terraform init`

`terraform validate`

`terraform apply`

`terraform destroy`

(Make sure the `prevent_destroy = true` in certificates is set to false before destroy)


Use `-var="profile=profile-name"` after command if running with IAM profile locally.
## Setup
Three phases:
### 1. Get Nameservers 
`terraform apply -target="aws_route53_zone.mj_zone_new"`
### 2. Apply Nameservers
Go to the domain provider and apply the name servers
### 3. Setup Rest
`terraform apply`

This will take a long time.