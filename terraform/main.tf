terraform {
  required_providers {
    aws = {
      source  = "hashicorp/aws"
      version = "5.98.0"
    }
  }
  backend "s3" {
    bucket  = "jeyanth-terraform-states"
    key     = "dotnet-multiverse/terraform.tfstate"
    region  = "eu-central-1"
    encrypt = true
    ## No DynamoDB to reduce cost
    # dynamodb_table = "terraform-lock-table"
  }
}

provider "aws" {
  region  = "eu-central-1"
  profile = var.profile
}

resource "aws_budgets_budget" "mj_monthly_budget" {
  name         = "MjBudget"
  budget_type  = "COST"
  limit_amount = "10"
  limit_unit   = "USD"
  time_unit    = "MONTHLY"
}

# Terraform state bucket
resource "aws_s3_bucket" "mj_state_bucket" {
  bucket = "dotnet-multiverse-bucket"
}

# Outputs 

output "route53_nameservers" {
  value = aws_route53_zone.mj_zone_new.name_servers
}
output "certificate_arn" {
  value = aws_acm_certificate.mj_certificate_new.arn
}
output "cloudfront_domain" {
  value = aws_cloudfront_distribution.mj_cdn.domain_name
}

output "bucket_name" {
  value = aws_s3_bucket.mj_state_bucket.bucket
}