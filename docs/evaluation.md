# Evaluation

Source: the BSc dissertation, Chapter 4. The numbers below match that chapter. They are not
revenue, usage or conversion metrics, and they do not claim generalised accuracy.

## What was evaluated

42 estimates produced by the system during iterative development:

| Recipe | Estimates |
|---|---|
| Kitchen | 18 iterations |
| Full apartment | 10 iterations |
| Bathroom, interior, terrace, facade | 14 combined |

Each estimate was checked against three criteria chosen to be objectively checkable:

1. **Geometric consistency** — computed areas respect the dimensions in the prompt.
2. **Price fairness** — unit prices fall within realistic local market ranges.
3. **Scope correctness** — the quote contains the items the user asked for, and nothing invented.

Validation was expert review by a domain expert, not systematic quality-assurance review. There
was no adjacent system to benchmark against and no comprehensive dataset of verified renovation
quotes. The method was chosen for repeatability and auditability, not statistical power.

## Kitchen

The kitchen recipe showed the clearest effect of moving computation from the model to C#.

| Metric | First iteration | Fifteenth iteration |
|---|---|---|
| Floor-screed hallucination | 271% | 0% |
| Tile-price deviation | 130% | 0% |
| Total-cost deviation | 39% | 0% |

The first iteration produced 44.53 m² of floor screed for a room that needs 12 m². Hallucinated
quantities multiplied by hallucinated prices produced the 39% total deviation.

Configuring the guardrails and moving the arithmetic into recipes reduced the kitchen total from
€6,537.72 to €4,714.89: **€1,822.83 of erroneous cost removed**.

## Full apartment

| Metric | Apartment #1 | Apartment #5 | Delta |
|---|---|---|---|
| Interior doors | 12 | 1 | −92% |
| Waste disposal | 11.4 m³ | 12 m³ | +5% |
| Total | €101,978.33 | €97,574.64 | −4.3% |

The request described a 65 m² interior, a 20 m² terrace and 360 m² of facade. The model read
"12 windows" in the facade section and leaked the number into the interior door count. A
blacklist and explicit routing fixed it. The waste quantity moved slightly *up*, from 11.4 to
12 m³, because geometry constraints corrected it; a deflection in the "wrong" direction that is
better aligned with the computed volume.

The total fell by €4,403.69.

## What the evaluation supports

- A boundary between model translation and deterministic computation was measurable, not
  theoretical: the same prompts converged toward verified values as computation moved into code.
- Hallucinated quantities and prices could be eliminated item by item.
- Scope errors were correctable with explicit routing rules rather than prompt tweaking alone.

## What it does not support

- **No holdout set.** The same 42 estimates were used while tuning. The reported improvements are
  in-sample. A fresh set would be a fairer test.
- **No statistical claim.** 42 estimates from one domain expert are a case series, not a
  controlled study.
- **One region, one business.** Prices and quantities reflect Tuscany and one cooperative's
  catalogue.
- **No measurement of the model-estimate fallback.** The evaluation measured the deterministic
  path; the labelled fallback for unmatched items was outside its scope.
- **No production A/B or usage data.** Production volume is owner-reported at 5–30 quotes/month.

The honest summary: the boundary worked on the material it was tuned on. It is not a claim of
generalised accuracy.
