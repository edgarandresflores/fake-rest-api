data "aws_ami" "ubuntu" {
  most_recent = true
  owners      = ["099720109477"] # Canonical

  filter {
    name   = "name"
    values = ["ubuntu/images/hvm-ssd/ubuntu-jammy-22.04-amd64-server-*"]
  }

  filter {
    name   = "virtualization-type"
    values = ["hvm"]
  }
}

data "aws_availability_zones" "available" {
  state = "available"
}

data "aws_subnet" "existing" {
  count = var.subnet_id != null ? 1 : 0
  id    = var.subnet_id
}

resource "aws_vpc" "this" {
  count = var.vpc_id == null && var.subnet_id == null ? 1 : 0

  cidr_block           = var.vpc_cidr
  enable_dns_support   = true
  enable_dns_hostnames = true

  tags = {
    Name = "${var.name}-vpc"
  }
}

resource "aws_internet_gateway" "this" {
  count = var.vpc_id == null && var.subnet_id == null ? 1 : 0

  vpc_id = aws_vpc.this[0].id

  tags = {
    Name = "${var.name}-igw"
  }
}

locals {
  effective_vpc_id = var.vpc_id != null ? var.vpc_id : (
    var.subnet_id != null ? data.aws_subnet.existing[0].vpc_id : aws_vpc.this[0].id
  )
  subnet_az                = coalesce(var.availability_zone, data.aws_availability_zones.available.names[0])
  effective_subnet_id      = var.subnet_id != null ? var.subnet_id : aws_subnet.this[0].id
  instance_user_data       = var.user_data_path == null ? null : file(var.user_data_path)
  final_user_data          = coalesce(local.instance_user_data, file("${path.module}/cloud.init.yaml"))
  normalized_key_pair_name = trimsuffix(var.key_pair_name, ".ppk")
  effective_public_key     = coalesce(var.key_pair_public_key, var.key_pair_public_key_path != null ? file(var.key_pair_public_key_path) : null)
  effective_key_pair_name  = local.effective_public_key != null ? aws_key_pair.kp_pz[0].key_name : local.normalized_key_pair_name
}

resource "aws_key_pair" "kp_pz" {
  count = local.effective_public_key != null ? 1 : 0

  key_name   = local.normalized_key_pair_name
  public_key = local.effective_public_key

  tags = {
    Name = local.normalized_key_pair_name
  }
}

resource "aws_subnet" "this" {
  count = var.subnet_id == null ? 1 : 0

  vpc_id                  = local.effective_vpc_id
  cidr_block              = var.subnet_cidr
  availability_zone       = local.subnet_az
  map_public_ip_on_launch = true

  tags = {
    Name = "${var.name}-subnet"
  }
}

resource "aws_route_table" "public" {
  count = var.subnet_id == null && var.vpc_id == null ? 1 : 0

  vpc_id = local.effective_vpc_id

  route {
    cidr_block = "0.0.0.0/0"
    gateway_id = aws_internet_gateway.this[0].id
  }

  tags = {
    Name = "${var.name}-public-rt"
  }
}

resource "aws_route_table_association" "public" {
  count = var.subnet_id == null && var.vpc_id == null ? 1 : 0

  subnet_id      = aws_subnet.this[0].id
  route_table_id = aws_route_table.public[0].id
}

resource "aws_security_group" "this" {
  name        = "${var.name}-sg"
  description = "SG for ${var.name}"
  vpc_id      = local.effective_vpc_id

  ingress {
    from_port   = 22
    to_port     = 22
    protocol    = "tcp"
    cidr_blocks = var.allowed_ingress_cidrs
    description = "Allow API traffic"
  }

  ingress {
    from_port   = var.api_port
    to_port     = var.api_port
    protocol    = "tcp"
    cidr_blocks = var.allowed_ingress_cidrs
    description = "Allow API traffic"
  }

  # Sin SSH por defecto (SSM no necesita 22 abierto)

  egress {
    from_port   = 0
    to_port     = 0
    protocol    = "-1"
    cidr_blocks = var.allowed_egress_cidrs
  }

  tags = {
    Name = "${var.name}-sg"
  }
}

resource "aws_iam_role" "ssm_role" {
  name = "${var.name}-role"

  assume_role_policy = jsonencode({
    Version = "2012-10-17"
    Statement = [{
      Effect    = "Allow"
      Principal = { Service = "ec2.amazonaws.com" }
      Action    = "sts:AssumeRole"
    }]
  })
}

resource "aws_iam_role_policy_attachment" "ssm_core" {
  role       = aws_iam_role.ssm_role.name
  policy_arn = "arn:aws:iam::aws:policy/AmazonSSMManagedInstanceCore"
}

resource "aws_iam_instance_profile" "this" {
  name = "${var.name}-instance-profile"
  role = aws_iam_role.ssm_role.name
}

resource "aws_instance" "this" {
  ami                         = data.aws_ami.ubuntu.id
  instance_type               = var.instance_type
  key_name                    = local.effective_key_pair_name
  subnet_id                   = local.effective_subnet_id
  vpc_security_group_ids      = [aws_security_group.this.id]
  iam_instance_profile        = aws_iam_instance_profile.this.name
  associate_public_ip_address = true

  user_data                   = local.final_user_data
  user_data_replace_on_change = true


  tags = {
    Name = var.name
  }
}
