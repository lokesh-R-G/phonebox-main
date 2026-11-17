const { test, expect } = require('@playwright/test');

test.use({ timezoneId: 'America/New_York' });

test('time progress uses the actual length of a day and week at the fall clock change', async ({ page }) => {
  // 23:30 local on the 25-hour day, still half an hour before Monday starts.
  await page.clock.setFixedTime(new Date('2026-11-02T04:30:00Z'));
  await page.addInitScript(() => {
    const config = JSON.stringify({
      night: { enabled: false }, rules: [], shortcuts: [],
      pages: [{ id: 'duration-test', title: 'Progress', widgets: [{ id: 'progress', type: 'progress', x: 0, y: 0, w: 12, h: 6 }] }],
    });
    localStorage.setItem('cfg', config);
    localStorage.setItem('pb-demo-config', config);
  });
  await page.goto('/panel/');

  const rows = page.locator('[data-id="duration-test"] .pr-row');
  await expect(rows.first()).toContainText('98%');
  const day = await rows.nth(0).locator('.track i').evaluate(el => parseFloat(el.style.width));
  const week = await rows.nth(1).locator('.track i').evaluate(el => parseFloat(el.style.width));
  expect(day).toBeCloseTo(98);
  expect(week).toBeCloseTo(100 * 168.5 / 169);
});
