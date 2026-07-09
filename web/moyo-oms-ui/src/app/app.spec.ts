import { routes } from './app.routes';

describe('Vendor portal routing', () => {
  it('redirects the root path to the dashboard', () => {
    const root = routes.find((route) => route.path === '');
    expect(root?.redirectTo).toBe('dashboard');
  });

  it('exposes the dashboard, products, and orders routes', () => {
    const paths = routes.map((route) => route.path);
    expect(paths).toContain('dashboard');
    expect(paths).toContain('products');
    expect(paths).toContain('orders');
    expect(paths).toContain('orders/:id');
  });
});
