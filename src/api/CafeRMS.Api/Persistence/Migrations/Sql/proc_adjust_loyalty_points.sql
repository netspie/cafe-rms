CREATE OR REPLACE PROCEDURE proc_adjust_loyalty_points(
    p_id         uuid,
    p_user_id    uuid,
    p_points     integer,
    p_reason     text,
    p_created_by uuid
)
LANGUAGE plpgsql
AS $$
DECLARE
    v_balance integer;
BEGIN
    IF p_points < 0 THEN
        v_balance := func_loyalty_balance(p_user_id);
        IF v_balance < -p_points THEN
            RAISE EXCEPTION 'insufficient loyalty balance';
        END IF;
    END IF;

    INSERT INTO loyalty_point_logs (id, user_id, points, reason, created_at, created_by)
    VALUES (p_id, p_user_id, p_points, p_reason, now(), p_created_by);
END;
$$;
