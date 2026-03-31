# Security groups
resource "aws_security_group" "mj_sg_aws_eu_prefix_list" {
  name        = "aws-eu-security-group"
  description = "Allow all traffic from AWS eu-central-1"
  vpc_id      = aws_vpc.mj_vpc.id
  ingress {
    description     = "Allow from prefix list"
    from_port       = 0
    to_port         = 0
    protocol        = "-1" # All protocols
    prefix_list_ids = ["pl-a3a144ca"]
  }

  egress {
    from_port   = 0
    to_port     = 0
    protocol    = "-1"
    cidr_blocks = ["0.0.0.0/0"]
  }

  timeouts {
    delete = "2m"
  }

  tags = {
    Name = "MjAwsSecurityGroup"
  }
}

resource "aws_security_group" "mj_sg_web" {
  name        = "web-security-group"
  description = "Allows HTTP inbound"
  vpc_id      = aws_vpc.mj_vpc.id
  ingress {
    description      = "HTTP"
    from_port        = 80
    to_port          = 80
    protocol         = "tcp"
    cidr_blocks      = var.cloudflare_ipv4
    ipv6_cidr_blocks = var.cloudflare_ipv6
  }

  egress {
    from_port   = 0
    to_port     = 0
    protocol    = "-1"
    cidr_blocks = ["0.0.0.0/0"]
  }

  timeouts {
    delete = "2m"
  }

  tags = {
    Name = "MjWebSecurityGroup"
  }
}

resource "aws_security_group" "mj_sg_ssh" {
  name        = "ssh-security-group"
  description = "Allows SSH inbound traffic (UNSAFE)"
  vpc_id      = aws_vpc.mj_vpc.id

  ingress {
    description = "SSH"
    from_port   = 22
    to_port     = 22
    protocol    = "tcp"
    cidr_blocks = var.ssh_allowed_cidrs
  }

  egress {
    from_port   = 0
    to_port     = 0
    protocol    = "-1"
    cidr_blocks = ["0.0.0.0/0"]
  }

  timeouts {
    delete = "2m"
  }

  tags = {
    Name = "MjSshSecurityGroup"
  }
}