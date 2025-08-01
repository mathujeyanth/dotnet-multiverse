provider "aws" {
  alias  = "us_east_1"
  region = "us-east-1"

  # Localstack
  /*  access_key = "test"
  secret_key = "test"
  skip_credentials_validation = true
  skip_metadata_api_check     = true
  skip_requesting_account_id  = true
  endpoints {
    acm = "http://localhost:4566"
  }*/
  profile = var.profile
}


# Get existing certificate
/*data "aws_acm_certificate" "mj_certificate_existing" {
  provider = aws.us_east_1 # certificate requires us-east-1
  domain = var.domain
  statuses = ["ISSUED"]
  most_recent = true
  key_types = ["EC_prime256v1"] # BUG: required otherwise aws cannot fetch it
}*/

# Create new certificate
resource "aws_acm_certificate" "mj_certificate_new" {
  provider = aws.us_east_1

  domain_name       = var.domain
  validation_method = "DNS"

  subject_alternative_names = ["www.${var.domain}"]

  key_algorithm = "EC_prime256v1"

  lifecycle {
    create_before_destroy = true
    # prevent_destroy       = true
  }

  tags = {
    Name = "MjCertificate"
  }
}

resource "aws_acm_certificate_validation" "mj_cert_validation" {
  provider                = aws.us_east_1
  certificate_arn         = aws_acm_certificate.mj_certificate_new.arn
  validation_record_fqdns = [for record in aws_route53_record.mj_cert_record : record.fqdn]
}