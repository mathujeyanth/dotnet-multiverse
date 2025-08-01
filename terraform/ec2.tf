# EC2 Instances:

## Get the latest Amazon Linux 2023
data "aws_ami" "al2023" {
  most_recent = true
  owners      = ["amazon"]

  filter {
    name   = "name"
    values = ["al2023-ami-2023.*-x86_64"]
  }
}

data "aws_key_pair" "mj_key" {
  key_name = "aws-kp-jeyanth"
}

resource "aws_instance" "mj_ec2" {
  ami                         = data.aws_ami.al2023.id
  instance_type               = "t2.micro"
  subnet_id                   = aws_subnet.mj_subnet.id
  associate_public_ip_address = true # false to disable public ip, but requires alb
  vpc_security_group_ids = [
    aws_security_group.mj_sg_web.id,
    aws_security_group.mj_sg_ssh.id,
    aws_security_group.mj_sg_aws_eu_prefix_list.id
  ]

  private_dns_name_options {
    enable_resource_name_dns_a_record = true
  }

  key_name = data.aws_key_pair.mj_key.key_name

  metadata_options {
    http_tokens   = "required"
    http_endpoint = "enabled"
  }
  user_data = file("${path.module}/setup_web_server.sh")

  root_block_device {
    volume_size = 15
  }

  tags = {
    Name = "MjEc2Instance"
  }
}