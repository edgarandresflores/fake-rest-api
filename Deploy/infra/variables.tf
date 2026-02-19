variable "aws_region" {
  type    = string
  default = "us-east-1"
}

variable "name" {
  type    = string
  default = "ubuntu-ssm"
}

variable "instance_type" {
  type    = string
  default = "t3.micro"
}

variable "subnet_id" {
  type = string
}

variable "vpc_id" {
  type = string
}

variable "allowed_egress_cidrs" {
  type    = list(string)
  default = ["0.0.0.0/0"]
}
