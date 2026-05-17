CREATE OR REPLACE VIEW view_loyalty_entry AS
SELECT
    l.id                                                                               AS entry_id,
    l.user_id                                                                          AS user_id,
    u.email                                                                            AS user_email,
    NULLIF(TRIM(COALESCE(u.first_name, '') || ' ' || COALESCE(u.last_name, '')), '')    AS user_name,
    l.points                                                                           AS points,
    l.reason                                                                           AS reason,
    l.created_at                                                                       AS created_at
FROM loyalty_point_logs l
LEFT JOIN asp_net_users u ON u.id = l.user_id AND u.deleted_at IS NULL;
