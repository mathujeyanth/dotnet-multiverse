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

  tags = {
    Name = "MjAwsSecurityGroup"
  }
}

resource "aws_security_group" "mj_sg_web" {
  name        = "web-security-group"
  description = "Allows HTTP inbound"
  vpc_id      = aws_vpc.mj_vpc.id
  ingress {
    description = "HTTP"
    from_port   = 80
    to_port     = 80
    protocol    = "tcp"
    cidr_blocks = ["0.0.0.0/0"]
  }
  egress {
    from_port   = 0
    to_port     = 0
    protocol    = "-1"
    cidr_blocks = ["0.0.0.0/0"]
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
    cidr_blocks = ["0.0.0.0/0"]
  }

  egress {
    from_port   = 0
    to_port     = 0
    protocol    = "-1"
    cidr_blocks = ["0.0.0.0/0"]
  }

  tags = {
    Name = "MjSshSecurityGroup"
  }
}