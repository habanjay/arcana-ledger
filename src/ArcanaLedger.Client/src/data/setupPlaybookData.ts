export type SetupCategory = 'Swing' | 'Momentum' | 'Position' | 'Capitulation';

export interface SetupPlaybook {
  name: string;
  category: SetupCategory;
  order: 'Long' | 'Short';
  derived: boolean;
  sourceNote: string;
  description: string;
  conditions: Array<[string, string]>;
  steps: Array<[string, string]>;
  params: { entry: string; cutloss: string; target: string; rr: string; timeframe: string; order: string; sizing: string; varPct: string };
  stats: Array<[string, string]>;
  checklist: string[];
  dos: string[];
  donts: string[];
  chart: { emaLabel: string; price: string; ema: string; entry: { x: number; y: number; label: string }; cutlossY: number; cutlossLabel: string; targetY: number; targetLabel: string };
}

const commonParams = (entry: string, cutloss: string, target: string, rr: string, timeframe: string, sizing = 'VAR 0.5% of capital / risk per share') => ({
  entry, cutloss, target, rr, timeframe, order: 'Long', sizing, varPct: sizing.startsWith('VAR 1%') ? '1% ($1,000 per $100k)' : '0.5% ($500 per $100k)',
});

export const setupPlaybooks: Record<string, SetupPlaybook> = {
  's-pullback': {
    name: 'S - Pullback', category: 'Swing', order: 'Long', derived: false,
    sourceNote: 'Documented - Caylum Trading Institute Swing Trading course and case study (ZM, SHOP).',
    description: 'A stock in a strong uptrend pulls back to the 10 EMA on light volume. The setup triggers on the first uptick day off the 10 EMA when that day range breaks to the upside.',
    conditions: [['Uptrend confirmed', 'Price above the 50 EMA and 200 EMA'], ['ADX > 30', 'Strong directional trend in place'], ['Pullback to the 10 EMA', 'Price retraces to touch or sit just above the 10 EMA'], ['Volume contracts on the pullback', 'Lighter than average volume while price retraces'], ['Prior swing high marked', 'Used as the 2R profit-taking reference']],
    steps: [['Confirm the trend', 'Check price is above both the 50 and 200 EMA with ADX > 30.'], ['Wait for the pullback', 'Let price retrace to the 10 EMA on light volume.'], ['Watch the uptick day', 'Mark the high and low range of the first up day.'], ['Enter on the break', 'Buy on a break above the uptick day high.'], ['Place the cutloss', 'Sell stop below the uptick day low.'], ['Manage the target', 'Sell at least 50% at 2R, then trail the rest.']],
    params: commonParams('$209.00 - break of the uptick-day high', '$196.00 - break of the uptick-day low', '$235.00 - 2R, sell 50% then trail', '2R min ($13 risk vs $26 reward)', 'Daily chart, hold 3-10 sessions'),
    stats: [['Case studies on file', '2 (ZM, SHOP)'], ['Target R-multiple', '2R min'], ['Avg position size', '8.6% of capital'], ['Typical hold', '3-10 sessions']],
    checklist: ['Price above 50 and 200 EMA', 'ADX reading above 30', 'Pullback touches the 10 EMA', 'Volume light on the pullback', 'Uptick day range marked', 'Cutloss sized to 0.5% VAR'],
    dos: ['Wait for the uptick-day range to break', 'Size the stop before entering', 'Take 50% off at 2R'], donts: ['Buy the pullback low pre-emptively', 'Trade it without ADX > 30', 'Skip the partial profit at 2R'],
    chart: { emaLabel: '10 EMA', price: '34,158 65,132 96,110 127,96 158,108 189,128 220,150 251,166 282,175 313,170 344,150 375,140 392,128 423,104 454,80 485,60 516,48 547,42 586,38', ema: '34,150 80,146 126,141 172,138 218,144 264,152 310,160 356,163 392,158 438,142 484,118 530,92 586,64', entry: { x: 392, y: 128, label: 'Entry $209.00' }, cutlossY: 178, cutlossLabel: 'Cutloss $196.00', targetY: 40, targetLabel: 'Target $235.00 (2R)' },
  },
  's-blue-sky': {
    name: 'S - Blue Sky Breakout', category: 'Swing', order: 'Long', derived: true,
    sourceNote: 'Adapted - derived from the Range Trade and Bluesky breakout concepts; illustrative example.',
    description: 'Price consolidates in a tight range at the 20 EMA with no resistance overhead. The setup triggers when price breaks the range top on expanding volume into new-high territory.',
    conditions: [['Uptrend in place', 'Price holding above the 20 EMA'], ['Tight consolidation range', 'Clearly defined high and low with little overlap'], ['Volume contracts then expands', 'Light volume in the range, pickup on breakout'], ['No overhead resistance', 'Breakout leads into new highs']],
    steps: [['Confirm the trend', 'Price above the 20 EMA (or 50 EMA if ADX is below 30).'], ['Mark the range', 'Draw the high and low of the consolidation.'], ['Wait for the breakout', 'Watch for the first uptick day to clear the range high.'], ['Enter on confirmation', 'Buy on the break of the range high.'], ['Place the cutloss', 'Sell stop below the range low.'], ['Manage the target', 'Sell 50% at 2R and trail the remainder.']],
    params: commonParams('$37.40 - break of the consolidation range high', '$35.80 - break of the range low', '$40.60 - 2R, sell 50% then trail', '2R min ($1.60 risk vs $3.20 reward)', 'Daily chart, hold 3-10 sessions'),
    stats: [['Case studies on file', '2 (RUN, ZS)'], ['Target R-multiple', '2R min'], ['Avg position size', '9.6% of capital'], ['Typical hold', '3-10 sessions']],
    checklist: ['Price above the 20 EMA', 'Consolidation range marked', 'Volume light inside range', 'Breakout volume expanding', 'Cutloss sized to 0.5% VAR'],
    dos: ['Wait for the range high to break', 'Match EMA reference to ADX strength', 'Trail into fresh highs'], donts: ['Chase price mid-range', 'Use a stop inside the range', 'Ignore ADX when picking the EMA'],
    chart: { emaLabel: '20 EMA', price: '34,172 70,150 106,130 142,118 178,114 214,116 250,113 286,117 322,114 358,111 386,95 410,74 438,54 466,38 494,26 522,18 550,12 586,8', ema: '34,166 90,148 150,126 214,118 280,116 340,114 386,108 440,86 500,52 560,20 586,10', entry: { x: 386, y: 95, label: 'Entry $37.40' }, cutlossY: 128, cutlossLabel: 'Cutloss $35.80', targetY: 16, targetLabel: 'Target $40.60 (2R)' },
  },
  'm-slingshot': {
    name: 'M - Slingshot', category: 'Momentum', order: 'Long', derived: false,
    sourceNote: 'Documented - Caylum Trading Institute Momentum Trading course and case study (LVGO, NIU).',
    description: 'After a high-volatility magnitude day, price pauses in a narrow range. The setup triggers when price breaks above that pause range on expanding volume.',
    conditions: [['Price above the 10 EMA', 'Basic trend filter'], ['Prior magnitude day', 'Expanded volume and wide MACD bands'], ['Pause day(s)', 'Narrow, quiet consolidation after the move'], ['Volume expands on breakout', 'Confirms the move is real'], ['ADX not required', 'Breakout itself confirms momentum']],
    steps: [['Spot the magnitude day', 'Look for a high-volatility day with expanded volume.'], ['Watch the pause', 'Mark the high and low of the quiet consolidation.'], ['Set the buy order', 'Place it just above the pause high.'], ['Enter on the break', 'Fill triggers when price clears the pause high.'], ['Place the cutloss', 'Sell stop at the pause low.'], ['Manage the target', 'Sell at least 50% at 2R and trail the remainder.']],
    params: commonParams('$34.20 - break of the pause-day high', '$31.70 - break of the pause-day low', '$39.20 - 2R, sell 50% then trail', '2R min ($2.50 risk vs $5.00 reward)', 'Daily chart, hold 3-8 sessions'),
    stats: [['Case studies on file', '2 (LVGO, NIU)'], ['Target R-multiple', '2R min'], ['Avg position size', '8.2% of capital'], ['Typical hold', '3-8 sessions']],
    checklist: ['Magnitude day identified', 'Pause range marked', 'Price above the 10 EMA', 'Breakout volume expanding', 'Cutloss sized to 0.5% VAR'],
    dos: ['Wait for the pause range to break', 'Confirm volume expansion', 'Take 50% off at 2R'], donts: ['Enter during the pause', 'Chase an extended breakout candle', 'Ignore the daily invalidation trail'],
    chart: { emaLabel: '10 EMA', price: '34,206 76,178 118,140 160,104 202,78 244,62 286,66 328,60 370,64 412,40 454,20 496,8 538,6 586,2', ema: '34,214 118,170 202,120 286,70 370,62 454,26 586,4', entry: { x: 370, y: 64, label: 'Entry $34.20' }, cutlossY: 70, cutlossLabel: 'Cutloss $31.70', targetY: 10, targetLabel: 'Target $39.20 (2R)' },
  },
  'm-expansion-breakout': {
    name: 'M - Expansion Breakout', category: 'Momentum', order: 'Long', derived: true,
    sourceNote: 'Derived - framework-based on the magnitude, pause, and ADX structure; illustrative example.',
    description: 'Daily range and volume contract for several sessions, squeezing volatility to a multi-week low. The setup triggers when a wide-range day closes above the contraction high on a sharp pickup in volume.',
    conditions: [['Price above the 10 EMA', 'Basic trend filter'], ['Range contraction', 'Daily range or ATR narrows for 5+ sessions'], ['Volume dries up', 'Below-average volume during the squeeze'], ['Expansion day', 'Wide-range close above the contraction high'], ['Volume spikes on the break', 'Confirms genuine expansion']],
    steps: [['Track volatility', 'Watch daily range and ATR contract over several sessions.'], ['Mark the squeeze', 'Define the high and low of the contraction zone.'], ['Wait for expansion', 'Look for a wide-range day pressing the contraction high.'], ['Enter on the break', 'Buy on the close above the contraction high.'], ['Place the cutloss', 'Sell stop below the contraction low.'], ['Manage the target', 'Sell at least 50% at 2R and trail the remainder.']],
    params: commonParams('$54.20 - close above the contraction high', '$51.40 - break of the contraction low', '$59.80 - 2R, sell 50% then trail', '2R min ($2.80 risk vs $5.60 reward)', 'Daily chart, hold 3-8 sessions'),
    stats: [['Case studies on file', '0 - illustrative only'], ['Target R-multiple', '2R min'], ['Avg position size', '~8% of capital'], ['Typical hold', '3-8 sessions']],
    checklist: ['Range/ATR contracting for 5+ sessions', 'Contraction high and low marked', 'Volume below average in squeeze', 'Expansion-day volume confirmed', 'Cutloss sized to 0.5% VAR'],
    dos: ['Wait for the contraction to fully form', 'Size the stop off the contraction low', 'Confirm volume on the break'], donts: ['Anticipate before the close', 'Use a loose stop inside the squeeze', 'Trade a squeeze with rising volume'],
    chart: { emaLabel: '10 EMA', price: '34,140 76,134 118,138 160,132 202,136 244,130 286,134 328,128 370,132 412,122 454,96 496,62 538,32 586,10', ema: '34,150 150,144 286,140 412,134 454,100 538,40 586,12', entry: { x: 412, y: 122, label: 'Entry $54.20' }, cutlossY: 146, cutlossLabel: 'Cutloss $51.40', targetY: 14, targetLabel: 'Target $59.80 (2R)' },
  },
  'm-jack-in-the-box': {
    name: 'M - Jack in the Box', category: 'Momentum', order: 'Long', derived: true,
    sourceNote: 'Derived - named for its tight micro-base and explosive release; illustrative example.',
    description: 'An extremely tight multi-day micro-base forms, then pops with a sudden breakout. The unusually small base creates tight risk and room for an extended move.',
    conditions: [['Price above the 10 EMA', 'Basic trend filter'], ['Three or more inside days', 'Very narrow ranges stacked together'], ['Quiet volume', 'Minimal participation while coiling'], ['Sharp breakout candle', 'Wide range closes above the base high'], ['Tight risk', 'Base range is unusually small']],
    steps: [['Spot the micro-base', 'Identify three or more tight, overlapping inside days.'], ['Mark the base', 'Define the narrow high and low precisely.'], ['Set the buy order', 'Place it just above the base high.'], ['Enter on the break', 'Fill triggers on the pop above the base.'], ['Place the cutloss', 'Sell stop below the base low.'], ['Manage the target', 'Extend to 3R given the tight stop and trail the remainder.']],
    params: commonParams('$28.90 - break of the micro-base high', '$28.10 - break of the micro-base low', '$31.30 - 3R given the tight stop', '3R target ($0.80 risk vs $2.40 reward)', 'Daily chart, hold 2-6 sessions'),
    stats: [['Case studies on file', '0 - illustrative only'], ['Target R-multiple', '3R'], ['Avg position size', '~6% of capital'], ['Typical hold', '2-6 sessions']],
    checklist: ['Three or more tight inside days', 'Base high and low marked', 'Volume quiet inside base', 'Breakout candle wide-range', 'Cutloss sized to 0.5% VAR'],
    dos: ['Confirm the base is unusually tight', 'Use small risk to extend the target', 'Act quickly once the pop triggers'], donts: ['Mistake a normal pause for a micro-base', 'Oversize because the stop is tight', 'Chase after the move extends'],
    chart: { emaLabel: '10 EMA', price: '34,150 90,148 146,150 202,147 258,149 314,147 358,146 388,104 416,62 444,28 472,8 520,4 586,2', ema: '34,152 150,150 258,149 358,148 388,118 444,46 520,10 586,2', entry: { x: 358, y: 146, label: 'Entry $28.90' }, cutlossY: 152, cutlossLabel: 'Cutloss $28.10', targetY: 10, targetLabel: 'Target $31.30 (3R)' },
  },
  'm-180-degrees': {
    name: 'M - 180 Degrees', category: 'Momentum', order: 'Long', derived: true,
    sourceNote: 'Derived - framework-based reversal concept named for a full trend reversal; illustrative example.',
    description: 'Price makes an extended climactic move, then reverses hard on a single wide-range day. The setup enters only after confirmation that the extreme has exhausted the prior trend.',
    conditions: [['Extended prior move', 'Price is climactic and stretched from the 10 EMA'], ['One-day reversal', 'Wide-range day closes opposite the prior trend'], ['Volume spike', 'Participation confirms the extreme'], ['Confirmation candle', 'Next session holds above the reversal low'], ['Mean target', 'Prior base or the 10/20 EMA is the first reference']],
    steps: [['Identify the extreme', 'Find price stretched well away from the 10 EMA.'], ['Watch for the reversal', 'Look for a wide-range reversal day on high volume.'], ['Wait for confirmation', 'The next session should hold the reversal range.'], ['Enter on the break', 'Buy above the reversal or confirmation high.'], ['Place the cutloss', 'Sell stop below the extreme low.'], ['Manage the target', 'Target the prior base or 10-20 EMA, then trail.']],
    params: commonParams('$46.60 - break above reversal-day high', '$41.90 - below the climactic extreme', '$55.00 - reclaim of the prior base', '~1.8R ($4.70 risk vs $8.40 reward)', 'Daily chart, hold 3-10 sessions'),
    stats: [['Case studies on file', '0 - illustrative only'], ['Target R-multiple', '~2R'], ['Avg position size', '~7% of capital'], ['Typical hold', '3-10 sessions']],
    checklist: ['Climactic extended move identified', 'Reversal confirmed on volume', 'Next session holds the range', 'Stop placed beyond the extreme', 'Cutloss sized to 0.5% VAR'],
    dos: ['Wait for the confirmation candle', 'Place the stop beyond the true extreme', 'Target the mean first, then trail'], donts: ['Catch the reversal day early', 'Use a stop inside the reversal range', 'Assume every reversal changes the trend'],
    chart: { emaLabel: '10 EMA', price: '34,36 76,58 118,86 160,116 202,146 244,172 286,196 328,212 370,160 412,118 454,86 496,60 538,38 586,20', ema: '34,48 160,120 286,198 328,210 412,150 496,70 586,24', entry: { x: 412, y: 118, label: 'Entry $46.60' }, cutlossY: 216, cutlossLabel: 'Cutloss $41.90', targetY: 28, targetLabel: 'Target $55.00' },
  },
  'm-boomer': {
    name: 'M - Boomer', category: 'Momentum', order: 'Long', derived: false,
    sourceNote: 'Documented - Caylum Trading Institute Momentum Trading course and case study (AMD, WIX).',
    description: 'The Boomer is the Slingshot with a stronger trend filter: the magnitude-day and pause structure only triggers when ADX confirms a genuinely strong trend.',
    conditions: [['ADX > 30 required', 'Key differentiator from the Slingshot'], ['DMI+ > DMI-', 'Confirms uptrend directional strength'], ['Price above the 10 EMA', 'Basic trend filter'], ['Prior magnitude day', 'Expanded volume and overbought MACD'], ['Pause forming', 'Quiet consolidation after the magnitude move']],
    steps: [['Confirm ADX > 30', 'A strong trend must already be in place.'], ['Spot the magnitude day', 'Look for expanded volume and overbought MACD.'], ['Watch the pause', 'Mark the high and low of the consolidation.'], ['Enter on the break', 'Buy above the pause high with ADX still above 30.'], ['Place the cutloss', 'Sell stop at the pause low.'], ['Manage the target', 'Sell at least 50% at 2R and extend on momentum.']],
    params: commonParams('$79.00 - break of the pause-day high', '$75.50 - break of the pause-day low', '$86.00 - 2R, sell 50% then trail', '2R min ($3.50 risk vs $7 reward)', 'Daily chart, hold 3-8 sessions'),
    stats: [['Case studies on file', '2 (AMD, WIX)'], ['Target R-multiple', '2R min (extendable)'], ['Avg position size', '10.5% of capital'], ['Typical hold', '3-8 sessions']],
    checklist: ['ADX reading above 30', 'DMI+ above DMI-', 'Magnitude day identified', 'Pause range marked', 'Cutloss sized to 0.5% VAR'],
    dos: ['Confirm ADX before entering', 'Wait for the pause range to break', 'Let strong moves extend past 2R'], donts: ['Trade it with ADX below 30', 'Enter before the range breaks', 'Cut the trail short on normal pullbacks'],
    chart: { emaLabel: '10 EMA / ADX > 30', price: '34,210 70,186 106,156 142,128 178,106 214,92 250,86 286,90 322,84 358,66 394,48 430,42 466,46 502,28 538,12 586,2', ema: '34,218 142,160 250,96 358,70 466,48 586,4', entry: { x: 466, y: 44, label: 'Entry $79.00' }, cutlossY: 52, cutlossLabel: 'Cutloss $75.50', targetY: 8, targetLabel: 'Target $86.00 (2R)' },
  },
  'm-123-pullback': {
    name: 'M - 1,2,3 Pullback', category: 'Momentum', order: 'Long', derived: true,
    sourceNote: 'Derived - framework-based on the classic three-push pullback structure; illustrative example.',
    description: 'Within an uptrend, price pulls back in three distinct legs, each finding a slightly lower low before a bounce. The setup triggers when the third leg bases and breaks higher.',
    conditions: [['Primary uptrend intact', 'Higher highs and higher lows on the larger timeframe'], ['Three pullback legs', 'Each leg has its own minor low and bounce'], ['Diminishing momentum', 'Range and volume lighten by the third leg'], ['Base at leg three', 'Brief consolidation at the final low'], ['10 EMA reclaimed', 'Price resumes trend on the break']],
    steps: [['Count the legs', 'Identify pushes 1, 2, and 3 within the pullback.'], ['Wait for leg three to base', 'Look for consolidation at the final low.'], ['Mark the base range', 'Define the high and low of that consolidation.'], ['Enter on the break', 'Buy above the base high.'], ['Place the cutloss', 'Sell stop below leg three low.'], ['Manage the target', 'Sell at least 50% at 2R and trail new highs.']],
    params: commonParams('$67.30 - break of the leg-three base high', '$63.80 - below leg three low', '$74.30 - 2R, sell 50% then trail', '2R min ($3.50 risk vs $7 reward)', 'Daily chart, hold 4-10 sessions'),
    stats: [['Case studies on file', '0 - illustrative only'], ['Target R-multiple', '2R min'], ['Avg position size', '~8% of capital'], ['Typical hold', '4-10 sessions']],
    checklist: ['Three pullback legs identified', 'Momentum diminishing by leg three', 'Base formed at final low', 'Price reclaims the 10 EMA', 'Cutloss sized to 0.5% VAR'],
    dos: ['Wait for all three legs', 'Confirm diminishing range and volume', 'Size the stop off leg three low'], donts: ['Enter on leg one or two', 'Assume two legs is enough', 'Ignore a leg three trend break'],
    chart: { emaLabel: '10 EMA', price: '34,198 76,158 118,126 150,146 182,130 214,152 246,138 278,160 310,148 342,148 374,122 416,92 458,62 500,36 542,16 586,4', ema: '34,206 118,160 214,150 278,158 342,150 374,138 458,70 542,20 586,4', entry: { x: 374, y: 122, label: 'Entry $67.30' }, cutlossY: 164, cutlossLabel: 'Cutloss $63.80', targetY: 20, targetLabel: 'Target $74.30 (2R)' },
  },
  'p-coil': {
    name: 'P - Coil', category: 'Position', order: 'Long', derived: false,
    sourceNote: 'Documented - Caylum Trading Institute Position Trading course and case study (PRLB, SPOT), the Falling Coil pattern.',
    description: 'Price grinds lower for 4-8 weeks inside a descending channel while MACD settles near zero. The Coil triggers when price breaks above diagonal resistance.',
    conditions: [['4-8 week duration', 'Descending consolidation after a prior move'], ['Diagonal resistance', 'Lower highs form a clear resistance line'], ['MACD near zero', 'Momentum settles during the grind'], ['Volume decreases', 'Participation contracts through the channel'], ['EMA stack reclaimed', 'Price moves above the 20 and 50 EMA']],
    steps: [['Confirm duration', 'Wait for the full 4-8 week channel to form.'], ['Draw resistance', 'Use actual swing highs for the diagonal line.'], ['Check MACD base', 'MACD should settle near zero.'], ['Enter on the break', 'Buy on a close above diagonal resistance.'], ['Place the cutloss', 'Sell stop below the duration low.'], ['Manage the target', 'Sell 50% at 2R, remainder at 4R, then trail.']],
    params: commonParams('$115.20 - close above diagonal resistance', '$106.70 - break of duration low', '$132.20 - 2R (50%), $149.20 - 4R', '2R / 4R split ($8.50 risk)', 'Weekly chart, hold 4-10 weeks', 'VAR 1% of capital / risk per share'),
    stats: [['Case studies on file', '2 (PRLB, SPOT)'], ['Target R-multiple', '2R / 4R split'], ['Avg position size', '14.1% of capital'], ['Typical hold', '4-10 weeks']],
    checklist: ['4-8 week descending channel', 'Diagonal resistance drawn', 'MACD sitting near zero', 'Volume decreasing', 'Cutloss sized to 1% VAR'],
    dos: ['Wait for the full duration', 'Draw resistance from real swing highs', 'Split exits between 2R and 4R'], donts: ['Buy mid-channel', 'Use a stop above the duration low', 'Treat it like a flat coil'],
    chart: { emaLabel: '20 EMA', price: '34,86 76,108 118,98 160,120 202,110 244,130 286,118 328,138 370,128 412,146 454,108 496,78 538,50 586,20', ema: '34,94 160,116 286,128 412,142 454,120 538,56 586,22', entry: { x: 454, y: 108, label: 'Entry $115.20' }, cutlossY: 150, cutlossLabel: 'Cutloss $106.70', targetY: 24, targetLabel: 'Target $132.20 (2R)' },
  },
  'p-base-duration': {
    name: 'P - Base 0 Duration', category: 'Position', order: 'Long', derived: false,
    sourceNote: 'Documented - Caylum Trading Institute Position Trading course and case study (NET, NTRA).',
    description: 'Price ranges sideways in a horizontal band for 4-8 weeks while MACD sits at its zero-line base. The setup triggers when price closes above horizontal resistance.',
    conditions: [['4-8 week flat range', 'Horizontal consolidation after a prior move'], ['Resistance clearly marked', 'Range high defines the trigger'], ['MACD near zero', 'Buyers and sellers reach equilibrium'], ['Volume decreases', 'Participation contracts through the range'], ['EMA stack reclaimed', 'Price above the 20 and 50 EMA on the break']],
    steps: [['Confirm duration', 'Wait for a complete 4-8 week range.'], ['Mark range boundaries', 'Define resistance and the duration low.'], ['Check MACD base', 'MACD should settle around zero.'], ['Enter on the close', 'Buy above horizontal resistance.'], ['Place the cutloss', 'Sell stop below the duration low.'], ['Manage the target', 'Sell 50% at 2R, remainder at 4R, then trail.']],
    params: commonParams('$30.00 - close above horizontal resistance', '$26.00 - break of duration low', '$38.00 - 2R (50%), $46.00 - 4R', '2R / 4R split ($4.00 risk)', 'Weekly chart, hold 4-10 weeks', 'VAR 1% of capital / risk per share'),
    stats: [['Case studies on file', '2 (NET, NTRA)'], ['Target R-multiple', '2R / 4R split'], ['Avg position size', '9.1% of capital'], ['Typical hold', '4-10 weeks']],
    checklist: ['4-8 week flat range identified', 'Horizontal resistance drawn', 'MACD sitting at zero base', 'Volume decreasing through range', 'Cutloss sized to 1% VAR'],
    dos: ['Confirm the range is truly flat', 'Wait for a decisive close', 'Split the exit between 2R and 4R'], donts: ['Buy a range still trending diagonally', 'Anticipate before the close', 'Use a stop inside the range'],
    chart: { emaLabel: '20 EMA', price: '34,118 76,114 118,122 160,116 202,124 244,116 286,122 328,114 370,120 412,116 454,92 496,66 538,42 586,16', ema: '34,120 150,118 286,120 412,118 454,104 538,54 586,18', entry: { x: 454, y: 92, label: 'Entry $30.00' }, cutlossY: 130, cutlossLabel: 'Cutloss $26.00', targetY: 20, targetLabel: 'Target $38.00 (2R)' },
  },
  'c-capitulation': {
    name: 'C - Capitulation', category: 'Capitulation', order: 'Long', derived: true,
    sourceNote: 'Derived - general market-timing concept adapted to the house VAR and EMA framework; illustrative example.',
    description: 'A sharp, high-volume sell-off drives price to a climactic low. The setup triggers after sellers are exhausted and the reversal day high breaks.',
    conditions: [['Volume spikes', 'At least 2-3x average on the sell-off'], ['Major support tested', 'Price reaches a meaningful demand zone'], ['Reversal candle', 'Strong reversal day closes off the low'], ['Confirmation break', 'Next session clears the reversal high'], ['Risk below the extreme', 'Cutloss sits beyond the true capitulation low']],
    steps: [['Identify the extreme', 'Find a sharp decline into major support.'], ['Wait for reversal', 'Look for a wide-range reversal day on high volume.'], ['Confirm the next session', 'The next session must hold the reversal range.'], ['Enter on the break', 'Buy above the reversal candle high.'], ['Place the cutloss', 'Sell stop below the capitulation low.'], ['Manage the target', 'First target the 10 or 20 EMA reclaim, then trail.']],
    params: commonParams('$22.40 - break above reversal-candle high', '$18.90 - below capitulation low', '$29.40 - 2R into EMA reclaim', '2R min ($3.50 risk vs $7.00 reward)', 'Daily chart, hold 3-10 sessions'),
    stats: [['Case studies on file', '0 - illustrative only'], ['Target R-multiple', '2R min'], ['Avg position size', '~7.5% of capital'], ['Typical hold', '3-10 sessions']],
    checklist: ['Volume 2-3x average', 'Major support tested', 'Reversal candle confirmed', 'Next session holds reversal low', 'Cutloss sized to 0.5% VAR'],
    dos: ['Wait for confirmation', 'Size below the true low', 'Scale out into the first reclaim'], donts: ['Pick the exact bottom', 'Use a stop inside the range', 'Ignore volume'],
    chart: { emaLabel: '20 EMA', price: '34,38 76,62 118,92 160,124 202,156 244,186 286,210 328,166 370,146 412,128 454,104 496,80 538,56 586,30', ema: '34,48 160,130 286,205 328,180 412,130 496,80 586,34', entry: { x: 328, y: 162, label: 'Entry $22.40' }, cutlossY: 214, cutlossLabel: 'Cutloss $18.90', targetY: 36, targetLabel: 'Target $29.40 (2R)' },
  },
  'c-square-root': {
    name: 'C - Square Root', category: 'Capitulation', order: 'Long', derived: true,
    sourceNote: 'Derived - named for the chart shape of a sharp drop, flat base, and gradual rise; illustrative example.',
    description: 'After a sharp decline, price forms a wide rounding bottom and slowly grinds higher. The setup triggers when price breaks the declining neckline of that base.',
    conditions: [['Sharp prior decline', 'The base follows a meaningful sell-off'], ['Rounding base', 'Base spans multiple weeks and flattens'], ['Neckline marked', 'Declining resistance joins base highs'], ['Volume trough', 'Volume is quietest near the midpoint'], ['Base is widening', 'Shape is wider than it is deep by the end']],
    steps: [['Confirm the decline', 'Identify the sharp move that precedes the base.'], ['Let the base round', 'Wait for several weeks of flattening price action.'], ['Draw the neckline', 'Connect the declining highs across the base.'], ['Enter on the break', 'Buy on a close above the neckline.'], ['Place the cutloss', 'Sell stop below the rounding-base low.'], ['Manage the target', 'Project a measured move, sell 50% at 2R, and trail.']],
    params: commonParams('$41.80 - close above the declining neckline', '$37.20 - below the rounding base low', '$51.00 - 2R (50%), $56.40 - 3R', '2R / 3R split ($4.60 risk)', 'Weekly chart, hold 6-12 weeks', 'VAR 1% of capital / risk per share'),
    stats: [['Case studies on file', '0 - illustrative only'], ['Target R-multiple', '2R / 3R split'], ['Avg position size', '~9% of capital'], ['Typical hold', '6-12 weeks']],
    checklist: ['Sharp prior decline confirmed', 'Rounding base spans weeks', 'Neckline drawn across highs', 'Volume trough near midpoint', 'Cutloss sized to 1% VAR'],
    dos: ['Let the base fully round', 'Use a gentle neckline slope', 'Place the stop below the whole base'], donts: ['Mistake a brief pause for a base', 'Buy before the neckline breaks', 'Expect a fast V-shaped recovery'],
    chart: { emaLabel: '50 EMA', price: '34,28 70,58 106,94 142,128 178,158 214,182 250,196 286,202 322,204 358,202 394,200 430,186 466,160 502,128 538,92 586,50', ema: '34,40 142,130 250,198 358,204 430,196 502,140 586,56', entry: { x: 430, y: 186, label: 'Entry $41.80' }, cutlossY: 208, cutlossLabel: 'Cutloss $37.20', targetY: 56, targetLabel: 'Target $51.00 (2R)' },
  },
};

export const setupCategoryClass: Record<SetupCategory, string> = { Swing: 'cat-swing', Momentum: 'cat-momentum', Position: 'cat-position', Capitulation: 'cat-capitulation' };