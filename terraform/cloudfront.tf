# CloudFront Distribution

data "aws_cloudfront_cache_policy" "use_origin_headers" {
  name = "UseOriginCacheControlHeaders"
}

data "aws_cloudfront_cache_policy" "managed_caching_optimized" {
  name = "Managed-CachingOptimized"
}

data "aws_cloudfront_origin_request_policy" "managed_all_viewer" {
  name = "Managed-AllViewer"
}

locals {
  common_methods = {
    allowed = ["GET", "HEAD", "OPTIONS", "PUT", "POST", "PATCH", "DELETE"]
    # cached  = ["GET", "HEAD", "OPTIONS", "PUT", "POST", "PATCH", "DELETE"]
    cached = ["GET", "HEAD", "OPTIONS"]

  }

  cache_behaviour_paths = [
    {
      path         = "/*.js"
      compress     = true
      cache_policy = data.aws_cloudfront_cache_policy.use_origin_headers.id
    },
    {
      path         = "/*.jpg"
      compress     = false
      cache_policy = data.aws_cloudfront_cache_policy.managed_caching_optimized.id
    },
    {
      path         = "/*.jpeg"
      compress     = false
      cache_policy = data.aws_cloudfront_cache_policy.managed_caching_optimized.id
    },
    {
      path         = "/*.gif"
      compress     = false
      cache_policy = data.aws_cloudfront_cache_policy.managed_caching_optimized.id
    },
    {
      path         = "/*.css"
      compress     = false
      cache_policy = data.aws_cloudfront_cache_policy.managed_caching_optimized.id
    },
    {
      path         = "/*.png"
      compress     = false
      cache_policy = data.aws_cloudfront_cache_policy.managed_caching_optimized.id
    }
  ]
}
locals {
  origin_id = "mj_ec2_origin"
}

resource "aws_cloudfront_distribution" "mj_cdn" {
  origin {
    origin_id   = local.origin_id
    domain_name = aws_instance.mj_ec2.public_dns
    custom_origin_config {
      http_port              = 80
      https_port             = 443
      origin_protocol_policy = "http-only"
      origin_ssl_protocols   = ["TLSv1.2"]
    }
    origin_shield {
      enabled              = true
      origin_shield_region = "eu-central-1"
    }
  }

  enabled = true
  # Enable IPv6
  is_ipv6_enabled = true

  viewer_certificate {
    acm_certificate_arn      = aws_acm_certificate.mj_certificate_new.arn
    ssl_support_method       = "sni-only" # Preferred validation method
    minimum_protocol_version = "TLSv1.2_2021"
  }
  aliases    = [var.domain, "www.${var.domain}"]
  depends_on = [aws_acm_certificate_validation.mj_cert_validation]
  dynamic "ordered_cache_behavior" {
    for_each = local.cache_behaviour_paths
    content {
      path_pattern             = ordered_cache_behavior.value.path
      target_origin_id         = local.origin_id
      allowed_methods          = local.common_methods.allowed
      cached_methods           = local.common_methods.cached
      viewer_protocol_policy   = "redirect-to-https"
      compress                 = ordered_cache_behavior.value.compress
      cache_policy_id          = ordered_cache_behavior.value.cache_policy
      origin_request_policy_id = data.aws_cloudfront_origin_request_policy.managed_all_viewer.id
    }
  }

  default_cache_behavior {
    allowed_methods          = local.common_methods.allowed
    cached_methods           = local.common_methods.cached
    target_origin_id         = local.origin_id
    viewer_protocol_policy   = "redirect-to-https"
    compress                 = true
    cache_policy_id          = data.aws_cloudfront_cache_policy.use_origin_headers.id
    origin_request_policy_id = data.aws_cloudfront_origin_request_policy.managed_all_viewer.id
  }

  restrictions {
    geo_restriction {
      restriction_type = "none"
    }
  }

  tags = {
    Name = "MjCdn"
  }
}