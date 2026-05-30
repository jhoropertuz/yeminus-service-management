-- =============================================================
-- Yeminus Service Management - Oracle Schema
-- Compatible with Oracle 19c / 21c / 23c
-- =============================================================

-- Drop tables if exist
BEGIN
    EXECUTE IMMEDIATE 'DROP TABLE order_status_history CASCADE CONSTRAINTS'; EXCEPTION WHEN OTHERS THEN NULL;
END;
/
BEGIN
    EXECUTE IMMEDIATE 'DROP TABLE service_orders CASCADE CONSTRAINTS'; EXCEPTION WHEN OTHERS THEN NULL;
END;
/
BEGIN
    EXECUTE IMMEDIATE 'DROP TABLE technicians CASCADE CONSTRAINTS'; EXCEPTION WHEN OTHERS THEN NULL;
END;
/
BEGIN
    EXECUTE IMMEDIATE 'DROP TABLE clients CASCADE CONSTRAINTS'; EXCEPTION WHEN OTHERS THEN NULL;
END;
/
BEGIN
    EXECUTE IMMEDIATE 'DROP TABLE user_roles CASCADE CONSTRAINTS'; EXCEPTION WHEN OTHERS THEN NULL;
END;
/
BEGIN
    EXECUTE IMMEDIATE 'DROP TABLE users CASCADE CONSTRAINTS'; EXCEPTION WHEN OTHERS THEN NULL;
END;
/
BEGIN
    EXECUTE IMMEDIATE 'DROP TABLE roles CASCADE CONSTRAINTS'; EXCEPTION WHEN OTHERS THEN NULL;
END;
/
BEGIN
    EXECUTE IMMEDIATE 'DROP TABLE persons CASCADE CONSTRAINTS'; EXCEPTION WHEN OTHERS THEN NULL;
END;
/
BEGIN
    EXECUTE IMMEDIATE 'DROP SEQUENCE roles_seq'; EXCEPTION WHEN OTHERS THEN NULL;
END;
/

-- =============================================================
-- PERSONS
-- =============================================================
CREATE TABLE persons (
    id              RAW(16)       DEFAULT SYS_GUID() PRIMARY KEY,
    full_name       VARCHAR2(200) NOT NULL,
    document_number VARCHAR2(20)  NOT NULL UNIQUE,
    phone           VARCHAR2(20)  NOT NULL,
    email           VARCHAR2(100) NOT NULL,
    created_at      TIMESTAMP     DEFAULT SYSTIMESTAMP NOT NULL,
    updated_at      TIMESTAMP     DEFAULT SYSTIMESTAMP NOT NULL
);
/

-- =============================================================
-- ROLES
-- =============================================================
CREATE SEQUENCE roles_seq START WITH 1 INCREMENT BY 1 NOCACHE NOCYCLE;
/

CREATE TABLE roles (
    id          NUMBER       DEFAULT roles_seq.NEXTVAL PRIMARY KEY,
    name        VARCHAR2(50) NOT NULL UNIQUE,
    description VARCHAR2(200)
);
/

-- =============================================================
-- USERS
-- =============================================================
CREATE TABLE users (
    id            RAW(16)       DEFAULT SYS_GUID() PRIMARY KEY,
    person_id     RAW(16)       NOT NULL REFERENCES persons(id) ON DELETE CASCADE,
    username      VARCHAR2(50)  NOT NULL UNIQUE,
    password_hash VARCHAR2(255) NOT NULL,
    is_active     NUMBER(1)     DEFAULT 1 NOT NULL CHECK (is_active IN (0, 1)),
    last_login    TIMESTAMP,
    created_at    TIMESTAMP     DEFAULT SYSTIMESTAMP NOT NULL
);
/

-- =============================================================
-- USER_ROLES
-- =============================================================
CREATE TABLE user_roles (
    user_id RAW(16) NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    role_id NUMBER  NOT NULL REFERENCES roles(id) ON DELETE CASCADE,
    CONSTRAINT pk_user_roles PRIMARY KEY (user_id, role_id)
);
/

-- =============================================================
-- CLIENTS
-- =============================================================
CREATE TABLE clients (
    id         RAW(16)       DEFAULT SYS_GUID() PRIMARY KEY,
    person_id  RAW(16)       NOT NULL REFERENCES persons(id) ON DELETE CASCADE,
    address    VARCHAR2(300) NOT NULL,
    created_at TIMESTAMP     DEFAULT SYSTIMESTAMP NOT NULL
);
/

-- =============================================================
-- TECHNICIANS
-- =============================================================
CREATE TABLE technicians (
    id         RAW(16)       DEFAULT SYS_GUID() PRIMARY KEY,
    person_id  RAW(16)       NOT NULL REFERENCES persons(id) ON DELETE CASCADE,
    specialty  VARCHAR2(100) NOT NULL,
    created_at TIMESTAMP     DEFAULT SYSTIMESTAMP NOT NULL
);
/

-- =============================================================
-- SERVICE_ORDERS
-- =============================================================
CREATE TABLE service_orders (
    id            RAW(16)        DEFAULT SYS_GUID() PRIMARY KEY,
    client_id     RAW(16)        NOT NULL REFERENCES clients(id),
    technician_id RAW(16)        NOT NULL REFERENCES technicians(id),
    status        NUMBER(1)      DEFAULT 1 NOT NULL CHECK (status IN (1, 2, 3)),
    description   VARCHAR2(1000) NOT NULL,
    created_by    RAW(16)        NOT NULL REFERENCES users(id),
    created_at    TIMESTAMP      DEFAULT SYSTIMESTAMP NOT NULL,
    updated_at    TIMESTAMP      DEFAULT SYSTIMESTAMP NOT NULL
);
/

-- =============================================================
-- ORDER_STATUS_HISTORY
-- =============================================================
CREATE TABLE order_status_history (
    id               RAW(16)   DEFAULT SYS_GUID() PRIMARY KEY,
    service_order_id RAW(16)   NOT NULL REFERENCES service_orders(id) ON DELETE CASCADE,
    old_status       NUMBER(1) NOT NULL,
    new_status       NUMBER(1) NOT NULL,
    changed_by       RAW(16)   NOT NULL REFERENCES users(id),
    changed_at       TIMESTAMP DEFAULT SYSTIMESTAMP NOT NULL
);
/

-- =============================================================
-- INDEXES
-- =============================================================
CREATE INDEX idx_so_client     ON service_orders(client_id);
CREATE INDEX idx_so_technician ON service_orders(technician_id);
CREATE INDEX idx_so_status     ON service_orders(status);
CREATE INDEX idx_so_created    ON service_orders(created_at DESC);
CREATE INDEX idx_osh_order     ON order_status_history(service_order_id);
/

-- =============================================================
-- SEED DATA
-- =============================================================

INSERT INTO roles (name, description) VALUES ('Admin', 'System administrator');
INSERT INTO roles (name, description) VALUES ('Operator', 'Service order operator');

COMMIT;
/

-- Note: For Oracle, UUID/RAW columns use SYS_GUID() which generates UUIDs automatically.
-- Insert seed users with pre-generated GUIDs:

INSERT INTO persons (id, full_name, document_number, phone, email) VALUES
(HEXTORAW('A1B2C3D4E5F67890ABCDEF1234567890'), 'System Administrator', '00000001', '+1000000000', 'admin@yeminus.com');

INSERT INTO users (id, person_id, username, password_hash, is_active) VALUES
(HEXTORAW('B2C3D4E5F6A78901BCDEF12345678901'),
 HEXTORAW('A1B2C3D4E5F67890ABCDEF1234567890'),
 'admin',
 '$2a$12$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/LewGJN6R5z0EHrPm2',
 1);

INSERT INTO user_roles (user_id, role_id) VALUES
(HEXTORAW('B2C3D4E5F6A78901BCDEF12345678901'), 1);

COMMIT;
/
