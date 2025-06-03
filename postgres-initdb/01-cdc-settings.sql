ALTER SYSTEM SET max_wal_senders = '10';
ALTER SYSTEM SET max_replication_slots = '10';
ALTER SYSTEM SET wal_keep_size = '512MB';
CREATE PUBLICATION pub FOR ALL TABLES;