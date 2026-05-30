-- =============================================================
-- Yeminus Service Management - PostgreSQL Schema
-- Los usuarios admin/operator son creados por DatabaseSeeder.cs
-- con hash BCrypt generado en runtime (no hardcodeado aquí).
-- =============================================================

DROP TABLE IF EXISTS order_status_history CASCADE;
DROP TABLE IF EXISTS service_orders CASCADE;
DROP TABLE IF EXISTS technicians CASCADE;
DROP TABLE IF EXISTS clients CASCADE;
DROP TABLE IF EXISTS user_roles CASCADE;
DROP TABLE IF EXISTS users CASCADE;
DROP TABLE IF EXISTS roles CASCADE;
DROP TABLE IF EXISTS persons CASCADE;

-- =============================================================
-- SCHEMA
-- =============================================================

CREATE TABLE persons (
    id              UUID         PRIMARY KEY DEFAULT gen_random_uuid(),
    full_name       VARCHAR(200) NOT NULL,
    document_number VARCHAR(20)  NOT NULL UNIQUE,
    phone           VARCHAR(20)  NOT NULL,
    email           VARCHAR(100) NOT NULL,
    created_at      TIMESTAMPTZ  NOT NULL DEFAULT NOW(),
    updated_at      TIMESTAMPTZ  NOT NULL DEFAULT NOW()
);

CREATE TABLE roles (
    id          SERIAL       PRIMARY KEY,
    name        VARCHAR(50)  NOT NULL UNIQUE,
    description VARCHAR(200)
);

CREATE TABLE users (
    id            UUID         PRIMARY KEY DEFAULT gen_random_uuid(),
    person_id     UUID         NOT NULL REFERENCES persons(id) ON DELETE CASCADE,
    username      VARCHAR(50)  NOT NULL UNIQUE,
    password_hash VARCHAR(255) NOT NULL,
    is_active     BOOLEAN      NOT NULL DEFAULT TRUE,
    last_login    TIMESTAMPTZ,
    created_at    TIMESTAMPTZ  NOT NULL DEFAULT NOW()
);

CREATE TABLE user_roles (
    user_id UUID    NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    role_id INTEGER NOT NULL REFERENCES roles(id) ON DELETE CASCADE,
    PRIMARY KEY (user_id, role_id)
);

CREATE TABLE clients (
    id         UUID         PRIMARY KEY DEFAULT gen_random_uuid(),
    person_id  UUID         NOT NULL REFERENCES persons(id) ON DELETE CASCADE,
    address    VARCHAR(300) NOT NULL,
    created_at TIMESTAMPTZ  NOT NULL DEFAULT NOW()
);

CREATE TABLE technicians (
    id         UUID         PRIMARY KEY DEFAULT gen_random_uuid(),
    person_id  UUID         NOT NULL REFERENCES persons(id) ON DELETE CASCADE,
    specialty  VARCHAR(100) NOT NULL,
    created_at TIMESTAMPTZ  NOT NULL DEFAULT NOW()
);

CREATE TABLE service_orders (
    id            UUID          PRIMARY KEY DEFAULT gen_random_uuid(),
    client_id     UUID          NOT NULL REFERENCES clients(id),
    technician_id UUID          NOT NULL REFERENCES technicians(id),
    status        SMALLINT      NOT NULL DEFAULT 1,
    description   VARCHAR(1000) NOT NULL,
    created_by    UUID          NOT NULL REFERENCES users(id),
    created_at    TIMESTAMPTZ   NOT NULL DEFAULT NOW(),
    updated_at    TIMESTAMPTZ   NOT NULL DEFAULT NOW()
);

CREATE TABLE order_status_history (
    id               UUID        PRIMARY KEY DEFAULT gen_random_uuid(),
    service_order_id UUID        NOT NULL REFERENCES service_orders(id) ON DELETE CASCADE,
    old_status       SMALLINT    NOT NULL,
    new_status       SMALLINT    NOT NULL,
    changed_by       UUID        NOT NULL REFERENCES users(id),
    changed_at       TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

-- =============================================================
-- INDEXES
-- =============================================================

CREATE INDEX idx_service_orders_client     ON service_orders(client_id);
CREATE INDEX idx_service_orders_technician ON service_orders(technician_id);
CREATE INDEX idx_service_orders_status     ON service_orders(status);
CREATE INDEX idx_service_orders_created_at ON service_orders(created_at DESC);
CREATE INDEX idx_order_history_order       ON order_status_history(service_order_id);
CREATE INDEX idx_users_username            ON users(username);
CREATE INDEX idx_persons_document          ON persons(document_number);

-- =============================================================
-- ROLES (requerido antes de que el seeder cree usuarios)
-- =============================================================

INSERT INTO roles (name, description) VALUES
    ('Admin',    'System administrator'),
    ('Operator', 'Service order operator');

-- =============================================================
-- CLIENTES Y TÉCNICOS DE MUESTRA
-- NOTA: las órdenes de muestra las genera el seeder (necesita el user_id del admin)
-- =============================================================

INSERT INTO persons (id, full_name, document_number, phone, email) VALUES
    ('11111111-1111-1111-1111-111111111111', 'Carlos Rodriguez', '12345678', '+57300000001', 'carlos.rodriguez@email.com'),
    ('22222222-2222-2222-2222-222222222222', 'Maria Lopez',       '87654321', '+57300000002', 'maria.lopez@email.com'),
    ('33333333-3333-3333-3333-333333333333', 'Juan Perez',        '11223344', '+57300000003', 'juan.perez@email.com');

INSERT INTO clients (id, person_id, address) VALUES
    ('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa', '11111111-1111-1111-1111-111111111111', 'Calle 100 #15-25, Bogotá'),
    ('bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb', '22222222-2222-2222-2222-222222222222', 'Carrera 50 #80-10, Medellín'),
    ('cccccccc-cccc-cccc-cccc-cccccccccccc', '33333333-3333-3333-3333-333333333333', 'Avenida 30 #20-05, Cali');

INSERT INTO persons (id, full_name, document_number, phone, email) VALUES
    ('44444444-4444-4444-4444-444444444444', 'Pedro Gomez',  '55667788', '+57310000001', 'pedro.gomez@yeminus.com'),
    ('55555555-5555-5555-5555-555555555555', 'Ana Martinez', '99887766', '+57310000002', 'ana.martinez@yeminus.com'),
    ('66666666-6666-6666-6666-666666666666', 'Luis Torres',  '44332211', '+57310000003', 'luis.torres@yeminus.com');

INSERT INTO technicians (id, person_id, specialty) VALUES
    ('dddddddd-dddd-dddd-dddd-dddddddddddd', '44444444-4444-4444-4444-444444444444', 'Electrical'),
    ('eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee', '55555555-5555-5555-5555-555555555555', 'Plumbing'),
    ('ffffffff-ffff-ffff-ffff-ffffffffffff', '66666666-6666-6666-6666-666666666666', 'HVAC');
