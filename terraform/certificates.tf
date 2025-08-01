provider "aws" {
  alias   = "us_east_1"
  region  = "us-east-1"
  profile = var.profile
}

# Create new certificate
resource "aws_acm_certificate" "mj_certificate_new" {
  provider = aws.us_east_1

  domain_name       = var.domain
  validation_method = "DNS"

  subject_alternative_names = ["www.${var.domain}"]

  key_algorithm = "EC_prime256v1"

  lifecycle {
    create_before_destroy = true
    prevent_destroy       = true
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