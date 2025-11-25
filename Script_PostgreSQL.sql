-- Category
CREATE TABLE categories (
    id              BIGSERIAL PRIMARY KEY,
    name            VARCHAR(255) NOT NULL,
    slug            VARCHAR(255) NOT NULL UNIQUE,
    parent_id       BIGINT REFERENCES categories(id),
    created_at      TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at      TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

-- Brand
CREATE TABLE brands (
    id              BIGSERIAL PRIMARY KEY,
    name            VARCHAR(255) NOT NULL UNIQUE,
    slug            VARCHAR(255) NOT NULL UNIQUE,
    created_at      TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at      TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

-- Product
CREATE TABLE products (
    id              BIGSERIAL PRIMARY KEY,
    name            VARCHAR(255) NOT NULL,
    slug            VARCHAR(255) NOT NULL UNIQUE,
    description     TEXT,
    category_id     BIGINT NOT NULL REFERENCES categories(id),
    brand_id        BIGINT REFERENCES brands(id),
    base_price      NUMERIC(12,2) NOT NULL CHECK (base_price >= 0),
    is_published    BOOLEAN NOT NULL DEFAULT FALSE,
    created_at      TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at      TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    row_version     BYTEA NOT NULL DEFAULT gen_random_uuid()::bytea
);

-- ProductVariant (SKU)
CREATE TABLE product_variants (
    id              BIGSERIAL PRIMARY KEY,
    product_id      BIGINT NOT NULL REFERENCES products(id) ON DELETE CASCADE,
    sku             VARCHAR(64) NOT NULL,
    color           VARCHAR(64),
    size            VARCHAR(64),
    additional_price NUMERIC(12,2) NOT NULL DEFAULT 0 CHECK (additional_price >= 0),
    is_active       BOOLEAN NOT NULL DEFAULT TRUE,
    created_at      TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at      TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    row_version     BYTEA NOT NULL DEFAULT gen_random_uuid()::bytea,
    UNIQUE (product_id, sku)
);

-- ProductImage
CREATE TABLE product_images (
    id              BIGSERIAL PRIMARY KEY,
    product_id      BIGINT NOT NULL REFERENCES products(id) ON DELETE CASCADE,
    image_url       TEXT NOT NULL,
    is_primary      BOOLEAN NOT NULL DEFAULT FALSE,
    sort_order      INT NOT NULL DEFAULT 0
);

-- Inventory per variant (per warehouse optional)
CREATE TABLE inventories (
    id              BIGSERIAL PRIMARY KEY,
    product_variant_id BIGINT NOT NULL REFERENCES product_variants(id) ON DELETE CASCADE,
    quantity        INT NOT NULL CHECK (quantity >= 0),
    reserved        INT NOT NULL DEFAULT 0 CHECK (reserved >= 0),
    updated_at      TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    row_version     BYTEA NOT NULL DEFAULT gen_random_uuid()::bytea,
    UNIQUE (product_variant_id)
);

CREATE INDEX idx_products_category_id ON products(category_id);
CREATE INDEX idx_products_brand_id ON products(brand_id);
CREATE INDEX idx_product_variants_product_id ON product_variants(product_id);
CREATE INDEX idx_inventories_variant_id ON inventories(product_variant_id);
CREATE INDEX idx_products_slug ON products(slug);

