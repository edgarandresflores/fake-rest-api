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

variable "key_pair_name" {
  type        = string
  default     = "kp-pz"
  description = "Nombre del key pair a usar/crear. Si termina en .ppk, se usa sin esa extension en AWS."
}

variable "key_pair_public_key" {
  type        = string
  default     = null
  description = "Public key en formato OpenSSH. Si se define, Terraform crea el key pair."
}

variable "key_pair_public_key_path" {
  type        = string
  default     = "kp-pz"
  description = "Ruta a archivo .pub (OpenSSH). Si se define y key_pair_public_key es null, Terraform lee la clave desde este archivo."
}

variable "subnet_id" {
  type        = string
  default     = null
  description = "Subnet existente. Si es null, Terraform crea una nueva subnet publica."
}

variable "vpc_id" {
  type        = string
  default     = null
  description = "VPC existente. Si es null, Terraform crea una nueva VPC."
}

variable "vpc_cidr" {
  type        = string
  default     = "10.40.0.0/16"
  description = "CIDR para la VPC cuando se crea automaticamente."
}

variable "subnet_cidr" {
  type        = string
  default     = "10.40.1.0/24"
  description = "CIDR para la subnet cuando se crea automaticamente."
}

variable "availability_zone" {
  type        = string
  default     = null
  description = "AZ para la subnet automatica. Si es null, se usa la primera disponible en la region."
}

variable "user_data_path" {
  type        = string
  default     = null
  description = "Ruta al archivo user_data custom. Si es null, usa Deploy/infra/cloud.init.yaml."
}

variable "allowed_egress_cidrs" {
  type    = list(string)
  default = ["0.0.0.0/0"]
}

variable "api_port" {
  type        = number
  default     = 80
  description = "Puerto de la API expuesto por la instancia."
}

variable "allowed_ingress_cidrs" {
  type        = list(string)
  default     = ["0.0.0.0/0"]
  description = "CIDRs permitidos para acceder al puerto de la API."
}
