
CREATE OR REPLACE FUNCTION public.get_project_total_cost(p_project_id uuid)
RETURNS numeric
LANGUAGE sql
STABLE
AS
$$
SELECT COALESCE(SUM(cost), 0)
FROM public.stages
WHERE project_id = p_project_id;
$$;
