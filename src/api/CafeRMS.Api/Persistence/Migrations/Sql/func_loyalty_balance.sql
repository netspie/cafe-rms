CREATE OR REPLACE FUNCTION func_loyalty_balance(p_user_id uuid)
RETURNS integer
LANGUAGE sql
AS $$
    SELECT COALESCE(SUM(points), 0)::integer
    FROM loyalty_point_logs
    WHERE user_id = p_user_id;
$$;
