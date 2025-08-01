# Route 53 DNS Validation

/*data "aws_route53_zone" "mj_zone_existing" {
  name = var.domain
  private_zone = false
}*/


resource "aws_route53_zone" "mj_zone_new" {
  # count = length(data.aws_route53_zone.mj_zone_existing) > 0 ? 0 : 1

  name = var.domain


  tags = {
    Name = "MjHostedZone"
  }
}

resource "aws_route53_record" "mj_cert_record" {
  for_each = {
    for dvo in aws_acm_certificate.mj_certificate_new.domain_validation_options : dvo.domain_name => {
      name   = dvo.resource_record_name
      type   = dvo.resource_record_type
      record = dvo.resource_record_value
    }
  }

  zone_id = aws_route53_zone.mj_zone_new.zone_id
  name    = each.value.name
  type    = each.value.type
  ttl     = 300
  records = [each.value.record]
}

resource "aws_route53_record" "mj_cloudfront_alias_record_1" {
  # count = length(aws_route53_zone.mj_zone_new) > 0 ? 1 : 0

  zone_id = aws_route53_zone.mj_zone_new.zone_id
  name    = var.domain
  type    = "A"
  alias {
    name                   = aws_cloudfront_distribution.mj_cdn.domain_name
    zone_id                = aws_cloudfront_distribution.mj_cdn.hosted_zone_id
    evaluate_target_health = false
  }
}

resource "aws_route53_record" "mj_cloudfront_alias_record_2" {
  count = length(aws_route53_zone.mj_zone_new) > 0 ? 1 : 0

  zone_id = aws_route53_zone.mj_zone_new.zone_id
  name    = "www.${var.domain}"
  type    = "A"
  alias {
    name                   = aws_cloudfront_distribution.mj_cdn.domain_name
    zone_id                = aws_cloudfront_distribution.mj_cdn.hosted_zone_id
    evaluate_target_health = false
  }
}