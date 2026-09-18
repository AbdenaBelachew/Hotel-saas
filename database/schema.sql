-- ============================================================
-- HOTEL & PROPERTY MANAGEMENT SaaS
-- PostgreSQL Database Schema
-- ============================================================

CREATE EXTENSION IF NOT EXISTS "pgcrypto";

-- ============================================================
-- 1. ORGANIZATIONS
-- ============================================================

CREATE TABLE organizations (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    name VARCHAR(200) NOT NULL,
    legal_name VARCHAR(250),
    code VARCHAR(50) NOT NULL UNIQUE,
    email VARCHAR(150),
    phone VARCHAR(50),
    address TEXT,
    city VARCHAR(100),
    country VARCHAR(100),
    logo_url TEXT,
    currency VARCHAR(10) NOT NULL DEFAULT 'ETB',
    timezone VARCHAR(100) NOT NULL DEFAULT 'Africa/Addis_Ababa',
    status VARCHAR(30) NOT NULL DEFAULT 'Active',
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ
);

-- ============================================================
-- 2. PROPERTIES
-- ============================================================

CREATE TABLE properties (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    organization_id UUID NOT NULL,
    name VARCHAR(200) NOT NULL,
    code VARCHAR(50) NOT NULL,
    property_type VARCHAR(50) NOT NULL DEFAULT 'Hotel',
    description TEXT,
    address TEXT,
    city VARCHAR(100),
    country VARCHAR(100),
    phone VARCHAR(50),
    email VARCHAR(150),
    website VARCHAR(250),
    logo_url TEXT,
    currency VARCHAR(10) NOT NULL DEFAULT 'ETB',
    timezone VARCHAR(100) NOT NULL DEFAULT 'Africa/Addis_Ababa',
    check_in_time TIME NOT NULL DEFAULT '14:00',
    check_out_time TIME NOT NULL DEFAULT '12:00',
    status VARCHAR(30) NOT NULL DEFAULT 'Active',
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ,

    CONSTRAINT fk_properties_organization
        FOREIGN KEY (organization_id)
        REFERENCES organizations(id)
        ON DELETE CASCADE,

    CONSTRAINT uq_property_code
        UNIQUE (organization_id, code)
);

CREATE INDEX idx_properties_organization
    ON properties(organization_id);

-- ============================================================
-- 3. USERS
-- ============================================================

CREATE TABLE users (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    organization_id UUID,
    username VARCHAR(100) NOT NULL UNIQUE,
    email VARCHAR(150) NOT NULL UNIQUE,
    password_hash TEXT NOT NULL,
    first_name VARCHAR(100) NOT NULL,
    middle_name VARCHAR(100),
    last_name VARCHAR(100),
    phone VARCHAR(50),
    is_active BOOLEAN NOT NULL DEFAULT TRUE,
    email_confirmed BOOLEAN NOT NULL DEFAULT FALSE,
    last_login_at TIMESTAMPTZ,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ,

    CONSTRAINT fk_users_organization
        FOREIGN KEY (organization_id)
        REFERENCES organizations(id)
        ON DELETE SET NULL
);

CREATE INDEX idx_users_organization
    ON users(organization_id);

-- ============================================================
-- 4. ROLES
-- ============================================================

CREATE TABLE roles (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    name VARCHAR(100) NOT NULL UNIQUE,
    description TEXT,
    is_system_role BOOLEAN NOT NULL DEFAULT FALSE,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

-- ============================================================
-- 5. PERMISSIONS
-- ============================================================

CREATE TABLE permissions (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    name VARCHAR(150) NOT NULL UNIQUE,
    description TEXT
);

-- ============================================================
-- 6. USER ROLES
-- ============================================================

CREATE TABLE user_roles (
    user_id UUID NOT NULL,
    role_id UUID NOT NULL,

    PRIMARY KEY (user_id, role_id),

    CONSTRAINT fk_user_roles_user
        FOREIGN KEY (user_id)
        REFERENCES users(id)
        ON DELETE CASCADE,

    CONSTRAINT fk_user_roles_role
        FOREIGN KEY (role_id)
        REFERENCES roles(id)
        ON DELETE CASCADE
);

-- ============================================================
-- 7. ROLE PERMISSIONS
-- ============================================================

CREATE TABLE role_permissions (
    role_id UUID NOT NULL,
    permission_id UUID NOT NULL,

    PRIMARY KEY (role_id, permission_id),

    CONSTRAINT fk_role_permissions_role
        FOREIGN KEY (role_id)
        REFERENCES roles(id)
        ON DELETE CASCADE,

    CONSTRAINT fk_role_permissions_permission
        FOREIGN KEY (permission_id)
        REFERENCES permissions(id)
        ON DELETE CASCADE
);

-- ============================================================
-- 8. USER PROPERTY ASSIGNMENT
-- ============================================================

CREATE TABLE user_properties (
    user_id UUID NOT NULL,
    property_id UUID NOT NULL,

    PRIMARY KEY (user_id, property_id),

    CONSTRAINT fk_user_properties_user
        FOREIGN KEY (user_id)
        REFERENCES users(id)
        ON DELETE CASCADE,

    CONSTRAINT fk_user_properties_property
        FOREIGN KEY (property_id)
        REFERENCES properties(id)
        ON DELETE CASCADE
);

-- ============================================================
-- 9. ROOM TYPES
-- ============================================================

CREATE TABLE room_types (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    property_id UUID NOT NULL,
    name VARCHAR(100) NOT NULL,
    code VARCHAR(50) NOT NULL,
    description TEXT,
    max_adults INT NOT NULL DEFAULT 2,
    max_children INT NOT NULL DEFAULT 0,
    base_rate NUMERIC(18,2) NOT NULL DEFAULT 0,
    extra_adult_rate NUMERIC(18,2) NOT NULL DEFAULT 0,
    extra_child_rate NUMERIC(18,2) NOT NULL DEFAULT 0,
    bed_type VARCHAR(100),
    number_of_beds INT NOT NULL DEFAULT 1,
    room_size NUMERIC(10,2),
    status VARCHAR(30) NOT NULL DEFAULT 'Active',
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ,

    CONSTRAINT fk_room_types_property
        FOREIGN KEY (property_id)
        REFERENCES properties(id)
        ON DELETE CASCADE,

    CONSTRAINT uq_room_type_code
        UNIQUE (property_id, code)
);

CREATE INDEX idx_room_types_property
    ON room_types(property_id);

-- ============================================================
-- 10. ROOMS
-- ============================================================

CREATE TABLE rooms (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    property_id UUID NOT NULL,
    room_type_id UUID NOT NULL,
    room_number VARCHAR(50) NOT NULL,
    floor VARCHAR(50),
    status VARCHAR(30) NOT NULL DEFAULT 'Available',
    housekeeping_status VARCHAR(30) NOT NULL DEFAULT 'Clean',
    notes TEXT,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ,

    CONSTRAINT fk_rooms_property
        FOREIGN KEY (property_id)
        REFERENCES properties(id)
        ON DELETE CASCADE,

    CONSTRAINT fk_rooms_room_type
        FOREIGN KEY (room_type_id)
        REFERENCES room_types(id)
        ON DELETE RESTRICT,

    CONSTRAINT uq_room_number
        UNIQUE (property_id, room_number)
);

CREATE INDEX idx_rooms_property
    ON rooms(property_id);

CREATE INDEX idx_rooms_room_type
    ON rooms(room_type_id);

CREATE INDEX idx_rooms_status
    ON rooms(status);

-- ============================================================
-- 11. ROOM AMENITIES
-- ============================================================

CREATE TABLE room_amenities (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    property_id UUID NOT NULL,
    name VARCHAR(100) NOT NULL,
    description TEXT,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),

    CONSTRAINT fk_room_amenities_property
        FOREIGN KEY (property_id)
        REFERENCES properties(id)
        ON DELETE CASCADE
);

CREATE TABLE room_type_amenities (
    room_type_id UUID NOT NULL,
    amenity_id UUID NOT NULL,

    PRIMARY KEY (room_type_id, amenity_id),

    CONSTRAINT fk_rta_room_type
        FOREIGN KEY (room_type_id)
        REFERENCES room_types(id)
        ON DELETE CASCADE,

    CONSTRAINT fk_rta_amenity
        FOREIGN KEY (amenity_id)
        REFERENCES room_amenities(id)
        ON DELETE CASCADE
);

-- ============================================================
-- 12. ROOM IMAGES
-- ============================================================

CREATE TABLE room_images (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    room_type_id UUID,
    room_id UUID,
    image_url TEXT NOT NULL,
    caption VARCHAR(250),
    sort_order INT NOT NULL DEFAULT 0,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),

    CONSTRAINT fk_room_images_room_type
        FOREIGN KEY (room_type_id)
        REFERENCES room_types(id)
        ON DELETE CASCADE,

    CONSTRAINT fk_room_images_room
        FOREIGN KEY (room_id)
        REFERENCES rooms(id)
        ON DELETE CASCADE
);

-- ============================================================
-- 13. GUESTS
-- ============================================================

CREATE TABLE guests (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    organization_id UUID NOT NULL,
    first_name VARCHAR(100) NOT NULL,
    middle_name VARCHAR(100),
    last_name VARCHAR(100),
    gender VARCHAR(30),
    date_of_birth DATE,
    nationality VARCHAR(100),
    passport_number VARCHAR(100),
    national_id VARCHAR(100),
    phone VARCHAR(50),
    email VARCHAR(150),
    address TEXT,
    city VARCHAR(100),
    country VARCHAR(100),
    company VARCHAR(200),
    notes TEXT,
    is_vip BOOLEAN NOT NULL DEFAULT FALSE,
    is_blacklisted BOOLEAN NOT NULL DEFAULT FALSE,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ,

    CONSTRAINT fk_guests_organization
        FOREIGN KEY (organization_id)
        REFERENCES organizations(id)
        ON DELETE CASCADE
);

CREATE INDEX idx_guests_organization
    ON guests(organization_id);

CREATE INDEX idx_guests_name
    ON guests(last_name, first_name);

CREATE INDEX idx_guests_phone
    ON guests(phone);

CREATE INDEX idx_guests_email
    ON guests(email);

-- ============================================================
-- 14. GUEST DOCUMENTS
-- ============================================================

CREATE TABLE guest_documents (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    guest_id UUID NOT NULL,
    document_type VARCHAR(50) NOT NULL,
    document_number VARCHAR(150),
    file_url TEXT,
    expiry_date DATE,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),

    CONSTRAINT fk_guest_documents_guest
        FOREIGN KEY (guest_id)
        REFERENCES guests(id)
        ON DELETE CASCADE
);

-- ============================================================
-- 15. RESERVATIONS
-- ============================================================

CREATE TABLE reservations (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    property_id UUID NOT NULL,
    reservation_number VARCHAR(50) NOT NULL,
    guest_id UUID NOT NULL,
    room_id UUID,
    room_type_id UUID NOT NULL,

    check_in_date DATE NOT NULL,
    check_out_date DATE NOT NULL,

    adults INT NOT NULL DEFAULT 1,
    children INT NOT NULL DEFAULT 0,

    rate NUMERIC(18,2) NOT NULL DEFAULT 0,
    tax NUMERIC(18,2) NOT NULL DEFAULT 0,
    discount NUMERIC(18,2) NOT NULL DEFAULT 0,
    total_amount NUMERIC(18,2) NOT NULL DEFAULT 0,
    paid_amount NUMERIC(18,2) NOT NULL DEFAULT 0,
    balance_amount NUMERIC(18,2) NOT NULL DEFAULT 0,

    status VARCHAR(30) NOT NULL DEFAULT 'Pending',
    source VARCHAR(50) NOT NULL DEFAULT 'WalkIn',

    special_requests TEXT,
    notes TEXT,

    created_by UUID,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ,

    CONSTRAINT fk_reservation_property
        FOREIGN KEY (property_id)
        REFERENCES properties(id)
        ON DELETE RESTRICT,

    CONSTRAINT fk_reservation_guest
        FOREIGN KEY (guest_id)
        REFERENCES guests(id)
        ON DELETE RESTRICT,

    CONSTRAINT fk_reservation_room
        FOREIGN KEY (room_id)
        REFERENCES rooms(id)
        ON DELETE RESTRICT,

    CONSTRAINT fk_reservation_room_type
        FOREIGN KEY (room_type_id)
        REFERENCES room_types(id)
        ON DELETE RESTRICT,

    CONSTRAINT fk_reservation_created_by
        FOREIGN KEY (created_by)
        REFERENCES users(id)
        ON DELETE SET NULL,

    CONSTRAINT uq_reservation_number
        UNIQUE (property_id, reservation_number),

    CONSTRAINT chk_reservation_dates
        CHECK (check_out_date > check_in_date),

    CONSTRAINT chk_reservation_guests
        CHECK (adults >= 1 AND children >= 0),

    CONSTRAINT chk_reservation_amounts
        CHECK (
            rate >= 0
            AND tax >= 0
            AND discount >= 0
            AND total_amount >= 0
            AND paid_amount >= 0
        )
);

CREATE INDEX idx_reservations_property
    ON reservations(property_id);

CREATE INDEX idx_reservations_guest
    ON reservations(guest_id);

CREATE INDEX idx_reservations_room
    ON reservations(room_id);

CREATE INDEX idx_reservations_dates
    ON reservations(check_in_date, check_out_date);

CREATE INDEX idx_reservations_status
    ON reservations(status);

-- ============================================================
-- 16. RESERVATION GUESTS
-- ============================================================

CREATE TABLE reservation_guests (
    reservation_id UUID NOT NULL,
    guest_id UUID NOT NULL,
    is_primary BOOLEAN NOT NULL DEFAULT FALSE,

    PRIMARY KEY (reservation_id, guest_id),

    CONSTRAINT fk_reservation_guests_reservation
        FOREIGN KEY (reservation_id)
        REFERENCES reservations(id)
        ON DELETE CASCADE,

    CONSTRAINT fk_reservation_guests_guest
        FOREIGN KEY (guest_id)
        REFERENCES guests(id)
        ON DELETE RESTRICT
);

-- ============================================================
-- 17. HOTEL SERVICES
-- ============================================================

CREATE TABLE hotel_services (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    property_id UUID NOT NULL,
    name VARCHAR(150) NOT NULL,
    service_type VARCHAR(50) NOT NULL,
    description TEXT,
    price NUMERIC(18,2) NOT NULL DEFAULT 0,
    tax_rate NUMERIC(8,2) NOT NULL DEFAULT 0,
    status VARCHAR(30) NOT NULL DEFAULT 'Active',
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),

    CONSTRAINT fk_hotel_services_property
        FOREIGN KEY (property_id)
        REFERENCES properties(id)
        ON DELETE CASCADE
);

-- ============================================================
-- 18. RESERVATION CHARGES
-- ============================================================

CREATE TABLE reservation_charges (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    reservation_id UUID NOT NULL,
    service_id UUID,
    description VARCHAR(250) NOT NULL,
    quantity NUMERIC(18,2) NOT NULL DEFAULT 1,
    unit_price NUMERIC(18,2) NOT NULL DEFAULT 0,
    tax_amount NUMERIC(18,2) NOT NULL DEFAULT 0,
    discount_amount NUMERIC(18,2) NOT NULL DEFAULT 0,
    total_amount NUMERIC(18,2) NOT NULL DEFAULT 0,
    charged_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    created_by UUID,

    CONSTRAINT fk_reservation_charges_reservation
        FOREIGN KEY (reservation_id)
        REFERENCES reservations(id)
        ON DELETE CASCADE,

    CONSTRAINT fk_reservation_charges_service
        FOREIGN KEY (service_id)
        REFERENCES hotel_services(id)
        ON DELETE SET NULL,

    CONSTRAINT fk_reservation_charges_user
        FOREIGN KEY (created_by)
        REFERENCES users(id)
        ON DELETE SET NULL
);

-- ============================================================
-- 19. PAYMENTS
-- ============================================================

CREATE TABLE payments (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    property_id UUID NOT NULL,
    reservation_id UUID,
    guest_id UUID,

    payment_number VARCHAR(50) NOT NULL,
    amount NUMERIC(18,2) NOT NULL,
    currency VARCHAR(10) NOT NULL DEFAULT 'ETB',

    payment_method VARCHAR(50) NOT NULL,
    payment_type VARCHAR(50) NOT NULL,

    reference_number VARCHAR(150),
    status VARCHAR(30) NOT NULL DEFAULT 'Completed',

    paid_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    received_by UUID,
    notes TEXT,

    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),

    CONSTRAINT fk_payments_property
        FOREIGN KEY (property_id)
        REFERENCES properties(id)
        ON DELETE RESTRICT,

    CONSTRAINT fk_payments_reservation
        FOREIGN KEY (reservation_id)
        REFERENCES reservations(id)
        ON DELETE SET NULL,

    CONSTRAINT fk_payments_guest
        FOREIGN KEY (guest_id)
        REFERENCES guests(id)
        ON DELETE SET NULL,

    CONSTRAINT fk_payments_user
        FOREIGN KEY (received_by)
        REFERENCES users(id)
        ON DELETE SET NULL,

    CONSTRAINT uq_payment_number
        UNIQUE (property_id, payment_number),

    CONSTRAINT chk_payment_amount
        CHECK (amount > 0)
);

CREATE INDEX idx_payments_reservation
    ON payments(reservation_id);

CREATE INDEX idx_payments_guest
    ON payments(guest_id);

CREATE INDEX idx_payments_date
    ON payments(paid_at);

-- ============================================================
-- 20. INVOICES
-- ============================================================

CREATE TABLE invoices (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    property_id UUID NOT NULL,
    reservation_id UUID,
    guest_id UUID,

    invoice_number VARCHAR(50) NOT NULL,
    invoice_date DATE NOT NULL DEFAULT CURRENT_DATE,
    due_date DATE,

    subtotal NUMERIC(18,2) NOT NULL DEFAULT 0,
    tax NUMERIC(18,2) NOT NULL DEFAULT 0,
    discount NUMERIC(18,2) NOT NULL DEFAULT 0,
    total NUMERIC(18,2) NOT NULL DEFAULT 0,
    paid NUMERIC(18,2) NOT NULL DEFAULT 0,
    balance NUMERIC(18,2) NOT NULL DEFAULT 0,

    status VARCHAR(30) NOT NULL DEFAULT 'Draft',

    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ,

    CONSTRAINT fk_invoices_property
        FOREIGN KEY (property_id)
        REFERENCES properties(id)
        ON DELETE RESTRICT,

    CONSTRAINT fk_invoices_reservation
        FOREIGN KEY (reservation_id)
        REFERENCES reservations(id)
        ON DELETE SET NULL,

    CONSTRAINT fk_invoices_guest
        FOREIGN KEY (guest_id)
        REFERENCES guests(id)
        ON DELETE SET NULL,

    CONSTRAINT uq_invoice_number
        UNIQUE (property_id, invoice_number)
);

-- ============================================================
-- 21. INVOICE ITEMS
-- ============================================================

CREATE TABLE invoice_items (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    invoice_id UUID NOT NULL,
    item_type VARCHAR(50) NOT NULL,
    description VARCHAR(250) NOT NULL,
    quantity NUMERIC(18,2) NOT NULL DEFAULT 1,
    unit_price NUMERIC(18,2) NOT NULL DEFAULT 0,
    tax_amount NUMERIC(18,2) NOT NULL DEFAULT 0,
    discount_amount NUMERIC(18,2) NOT NULL DEFAULT 0,
    total_amount NUMERIC(18,2) NOT NULL DEFAULT 0,

    CONSTRAINT fk_invoice_items_invoice
        FOREIGN KEY (invoice_id)
        REFERENCES invoices(id)
        ON DELETE CASCADE
);

-- ============================================================
-- 22. HOUSEKEEPING TASKS
-- ============================================================

CREATE TABLE housekeeping_tasks (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    property_id UUID NOT NULL,
    room_id UUID NOT NULL,

    task_type VARCHAR(50) NOT NULL DEFAULT 'Cleaning',
    assigned_to UUID,
    priority VARCHAR(30) NOT NULL DEFAULT 'Normal',
    status VARCHAR(30) NOT NULL DEFAULT 'Pending',

    started_at TIMESTAMPTZ,
    completed_at TIMESTAMPTZ,
    notes TEXT,

    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ,

    CONSTRAINT fk_housekeeping_property
        FOREIGN KEY (property_id)
        REFERENCES properties(id)
        ON DELETE CASCADE,

    CONSTRAINT fk_housekeeping_room
        FOREIGN KEY (room_id)
        REFERENCES rooms(id)
        ON DELETE CASCADE,

    CONSTRAINT fk_housekeeping_user
        FOREIGN KEY (assigned_to)
        REFERENCES users(id)
        ON DELETE SET NULL
);

CREATE INDEX idx_housekeeping_property
    ON housekeeping_tasks(property_id);

CREATE INDEX idx_housekeeping_room
    ON housekeeping_tasks(room_id);

CREATE INDEX idx_housekeeping_status
    ON housekeeping_tasks(status);

-- ============================================================
-- 23. MAINTENANCE REQUESTS
-- ============================================================

CREATE TABLE maintenance_requests (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    property_id UUID NOT NULL,
    room_id UUID,

    reported_by UUID,
    assigned_to UUID,

    issue_type VARCHAR(100) NOT NULL,
    priority VARCHAR(30) NOT NULL DEFAULT 'Normal',
    description TEXT NOT NULL,

    status VARCHAR(30) NOT NULL DEFAULT 'Open',

    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    resolved_at TIMESTAMPTZ,
    resolution TEXT,

    CONSTRAINT fk_maintenance_property
        FOREIGN KEY (property_id)
        REFERENCES properties(id)
        ON DELETE CASCADE,

    CONSTRAINT fk_maintenance_room
        FOREIGN KEY (room_id)
        REFERENCES rooms(id)
        ON DELETE SET NULL,

    CONSTRAINT fk_maintenance_reporter
        FOREIGN KEY (reported_by)
        REFERENCES users(id)
        ON DELETE SET NULL,

    CONSTRAINT fk_maintenance_assignee
        FOREIGN KEY (assigned_to)
        REFERENCES users(id)
        ON DELETE SET NULL
);

-- ============================================================
-- 24. NOTIFICATIONS
-- ============================================================

CREATE TABLE notifications (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id UUID NOT NULL,
    title VARCHAR(250) NOT NULL,
    message TEXT NOT NULL,
    notification_type VARCHAR(50),
    is_read BOOLEAN NOT NULL DEFAULT FALSE,
    read_at TIMESTAMPTZ,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),

    CONSTRAINT fk_notifications_user
        FOREIGN KEY (user_id)
        REFERENCES users(id)
        ON DELETE CASCADE
);

CREATE INDEX idx_notifications_user
    ON notifications(user_id, is_read);

-- ============================================================
-- 25. AUDIT LOGS
-- ============================================================

CREATE TABLE audit_logs (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),

    organization_id UUID,
    property_id UUID,
    user_id UUID,

    action VARCHAR(100) NOT NULL,
    entity_name VARCHAR(150),
    entity_id UUID,

    old_values JSONB,
    new_values JSONB,

    ip_address INET,
    user_agent TEXT,

    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),

    CONSTRAINT fk_audit_organization
        FOREIGN KEY (organization_id)
        REFERENCES organizations(id)
        ON DELETE SET NULL,

    CONSTRAINT fk_audit_property
        FOREIGN KEY (property_id)
        REFERENCES properties(id)
        ON DELETE SET NULL,

    CONSTRAINT fk_audit_user
        FOREIGN KEY (user_id)
        REFERENCES users(id)
        ON DELETE SET NULL
);

CREATE INDEX idx_audit_organization
    ON audit_logs(organization_id);

CREATE INDEX idx_audit_property
    ON audit_logs(property_id);

CREATE INDEX idx_audit_user
    ON audit_logs(user_id);

CREATE INDEX idx_audit_created
    ON audit_logs(created_at);

-- ============================================================
-- 26. REFRESH TOKENS
-- ============================================================

CREATE TABLE refresh_tokens (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id UUID NOT NULL,
    token_hash TEXT NOT NULL UNIQUE,
    expires_at TIMESTAMPTZ NOT NULL,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    revoked_at TIMESTAMPTZ,
    replaced_by_token_id UUID,

    CONSTRAINT fk_refresh_tokens_user
        FOREIGN KEY (user_id)
        REFERENCES users(id)
        ON DELETE CASCADE
);

CREATE INDEX idx_refresh_tokens_user
    ON refresh_tokens(user_id);

-- ============================================================
-- 27. SUBSCRIPTION PLANS
-- ============================================================

CREATE TABLE subscription_plans (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    name VARCHAR(100) NOT NULL UNIQUE,
    description TEXT,
    monthly_price NUMERIC(18,2) NOT NULL DEFAULT 0,
    yearly_price NUMERIC(18,2) NOT NULL DEFAULT 0,

    max_properties INT,
    max_rooms INT,
    max_users INT,

    storage_limit_mb INT,

    api_access BOOLEAN NOT NULL DEFAULT FALSE,
    advanced_reports BOOLEAN NOT NULL DEFAULT FALSE,

    is_active BOOLEAN NOT NULL DEFAULT TRUE,

    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

-- ============================================================
-- 28. ORGANIZATION SUBSCRIPTIONS
-- ============================================================

CREATE TABLE organization_subscriptions (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),

    organization_id UUID NOT NULL,
    subscription_plan_id UUID NOT NULL,

    start_date DATE NOT NULL,
    end_date DATE,

    status VARCHAR(30) NOT NULL DEFAULT 'Active',

    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),

    CONSTRAINT fk_org_subscription_org
        FOREIGN KEY (organization_id)
        REFERENCES organizations(id)
        ON DELETE CASCADE,

    CONSTRAINT fk_org_subscription_plan
        FOREIGN KEY (subscription_plan_id)
        REFERENCES subscription_plans(id)
        ON DELETE RESTRICT
);

-- ============================================================
-- 29. SEED ROLES
-- ============================================================

INSERT INTO roles (name, description, is_system_role)
VALUES
('PlatformAdmin', 'Platform administrator', TRUE),
('OrganizationAdmin', 'Organization administrator', TRUE),
('PropertyAdmin', 'Property administrator', TRUE),
('GeneralManager', 'General manager', TRUE),
('FrontDesk', 'Front desk staff', TRUE),
('ReservationAgent', 'Reservation agent', TRUE),
('Accountant', 'Accounting staff', TRUE),
('Housekeeper', 'Housekeeping staff', TRUE),
('Maintenance', 'Maintenance staff', TRUE),
('Manager', 'Manager', TRUE),
('ReportViewer', 'Report viewer', TRUE)
ON CONFLICT (name) DO NOTHING;

-- ============================================================
-- 30. SEED PERMISSIONS
-- ============================================================

INSERT INTO permissions (name, description)
VALUES
('properties.view', 'View properties'),
('properties.create', 'Create properties'),
('properties.update', 'Update properties'),
('properties.delete', 'Delete properties'),

('rooms.view', 'View rooms'),
('rooms.create', 'Create rooms'),
('rooms.update', 'Update rooms'),
('rooms.delete', 'Delete rooms'),

('reservations.view', 'View reservations'),
('reservations.create', 'Create reservations'),
('reservations.update', 'Update reservations'),
('reservations.cancel', 'Cancel reservations'),

('guests.view', 'View guests'),
('guests.create', 'Create guests'),
('guests.update', 'Update guests'),

('payments.view', 'View payments'),
('payments.create', 'Create payments'),
('payments.refund', 'Refund payments'),

('housekeeping.view', 'View housekeeping'),
('housekeeping.update', 'Update housekeeping'),

('reports.view', 'View reports'),
('reports.export', 'Export reports'),

('users.view', 'View users'),
('users.create', 'Create users'),
('users.update', 'Update users'),
('users.delete', 'Delete users')
ON CONFLICT (name) DO NOTHING;

-- ============================================================
-- 31. SEED SUBSCRIPTION PLANS
-- ============================================================

INSERT INTO subscription_plans
(
    name,
    description,
    monthly_price,
    yearly_price,
    max_properties,
    max_rooms,
    max_users,
    storage_limit_mb,
    api_access,
    advanced_reports
)
VALUES
(
    'Starter',
    'For small properties',
    0,
    0,
    1,
    30,
    5,
    1000,
    FALSE,
    FALSE
),
(
    'Professional',
    'For growing hotel businesses',
    99,
    999,
    5,
    200,
    25,
    10000,
    TRUE,
    TRUE
),
(
    'Enterprise',
    'For hotel groups and large organizations',
    299,
    2999,
    NULL,
    NULL,
    NULL,
    100000,
    TRUE,
    TRUE
)
ON CONFLICT (name) DO NOTHING;

-- ============================================================
-- 32. USEFUL VIEWS
-- ============================================================

-- Room availability overview

CREATE OR REPLACE VIEW vw_room_status AS
SELECT
    r.id AS room_id,
    r.property_id,
    p.name AS property_name,
    r.room_number,
    rt.name AS room_type,
    r.status AS room_status,
    r.housekeeping_status
FROM rooms r
JOIN properties p
    ON p.id = r.property_id
JOIN room_types rt
    ON rt.id = r.room_type_id;

-- Reservation financial summary

CREATE OR REPLACE VIEW vw_reservation_financials AS
SELECT
    r.id,
    r.property_id,
    r.reservation_number,
    r.guest_id,
    r.check_in_date,
    r.check_out_date,
    r.status,
    r.total_amount,
    COALESCE(SUM(
        CASE
            WHEN p.status = 'Completed'
            THEN p.amount
            ELSE 0
        END
    ), 0) AS calculated_paid_amount,
    r.total_amount -
    COALESCE(SUM(
        CASE
            WHEN p.status = 'Completed'
            THEN p.amount
            ELSE 0
        END
    ), 0) AS calculated_balance
FROM reservations r
LEFT JOIN payments p
    ON p.reservation_id = r.id
GROUP BY
    r.id;

-- ============================================================
-- 33. RESERVATION AVAILABILITY FUNCTION
-- ============================================================

CREATE OR REPLACE FUNCTION check_room_availability(
    p_room_id UUID,
    p_check_in DATE,
    p_check_out DATE,
    p_exclude_reservation_id UUID DEFAULT NULL
)
RETURNS BOOLEAN
LANGUAGE plpgsql
AS $$
DECLARE
    reservation_exists BOOLEAN;
BEGIN

    SELECT EXISTS (
        SELECT 1
        FROM reservations r
        WHERE r.room_id = p_room_id
          AND r.status IN ('Pending', 'Confirmed', 'CheckedIn')
          AND r.check_in_date < p_check_out
          AND r.check_out_date > p_check_in
          AND (
              p_exclude_reservation_id IS NULL
              OR r.id <> p_exclude_reservation_id
          )
    )
    INTO reservation_exists;

    RETURN NOT reservation_exists;

END;
$$;

-- ============================================================
-- 34. RESERVATION NUMBER GENERATOR
-- ============================================================

CREATE OR REPLACE FUNCTION generate_reservation_number()
RETURNS TEXT
LANGUAGE plpgsql
AS $$
BEGIN
    RETURN 'RES-' ||
           TO_CHAR(CURRENT_DATE, 'YYYYMMDD') ||
           '-' ||
           UPPER(SUBSTRING(REPLACE(gen_random_uuid()::TEXT, '-', '') FROM 1 FOR 6));
END;
$$;

-- ============================================================
-- 35. INVOICE NUMBER GENERATOR
-- ============================================================

CREATE OR REPLACE FUNCTION generate_invoice_number()
RETURNS TEXT
LANGUAGE plpgsql
AS $$
BEGIN
    RETURN 'INV-' ||
           TO_CHAR(CURRENT_DATE, 'YYYYMMDD') ||
           '-' ||
           UPPER(SUBSTRING(REPLACE(gen_random_uuid()::TEXT, '-', '') FROM 1 FOR 6));
END;
$$;

-- ============================================================
-- END
-- ============================================================
