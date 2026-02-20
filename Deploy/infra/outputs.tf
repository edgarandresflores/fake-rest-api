output "vpc_id" {
  value       = local.effective_vpc_id
  description = "VPC usada por el deploy (existente o creada)."
}

output "subnet_id" {
  value       = local.effective_subnet_id
  description = "Subnet usada por el deploy (existente o creada)."
}

output "instance_public_ip" {
  value       = aws_instance.this.public_ip
  description = "IP publica de la instancia EC2."
}

output "api_base_url" {
  value       = "http://${aws_instance.this.public_ip}"
  description = "Base URL de la API desplegada."
}

output "api_path_parameter_example" {
  value       = "http://${aws_instance.this.public_ip}/users/1"
  description = "Ejemplo de endpoint con path parameter."
}

output "key_pair_name" {
  value       = local.effective_key_pair_name
  description = "Key pair configurado en la instancia."
}
