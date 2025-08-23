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
  ami                         = "ami-0a72753edf3e631b7" // For latest use: data.aws_ami.al2023.id
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

  user_data = templatefile("${path.module}/setup_web_server.sh.tmpl", {
    docker_image = var.docker_image
  })

  root_block_device {
    volume_size = 15
  }

  tags = {
    Name = "MjEc2Instance"
  }
}

# Updating the docker image on change
resource "null_resource" "mj_update_docker_container" {
  triggers = {
    docker_image = var.docker_image
  }

  connection {
    type        = "ssh"
    user        = "ec2-user"
    host        = aws_instance.mj_ec2.public_ip
    private_key = var.ec2_private_key
  }

  provisioner "remote-exec" {
    inline = [
      "set -x",
      # Install Docker if not installed
      "command -v docker || (sudo yum install -y docker && sudo systemctl enable docker && sudo systemctl start docker && sudo usermod -aG docker ec2-user)",
      # Now run your container
      "sudo docker pull ${var.docker_image}",
      "sudo docker stop $(docker ps -aq) || true", # Maybe a bit overkill
      "sudo docker rm $(docker ps -aq) || true",
      "sudo docker run --name mj-dotnet-multiverse -d --restart always -p 80:8080 ${var.docker_image}"
    ]
  }
}
