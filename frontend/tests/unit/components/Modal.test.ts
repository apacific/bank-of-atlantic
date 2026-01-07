import { describe, it, expect } from 'vitest';
import { mount } from '@vue/test-utils';
import Modal from '@/components/Modal.vue';

describe('Modal Component', () => {
  it('should not render when open is false', () => {
    const wrapper = mount(Modal, {
      props: {
        open: false,
        title: 'Test Title',
        message: 'Test Message',
      },
    });

    expect(wrapper.find('.overlay').exists()).toBe(false);
  });

  it('should render when open is true', () => {
    const wrapper = mount(Modal, {
      props: {
        open: true,
        title: 'Test Title',
        message: 'Test Message',
      },
    });

    expect(wrapper.find('.overlay').exists()).toBe(true);
    expect(wrapper.find('.modal').exists()).toBe(true);
  });

  it('should display title and message', () => {
    const wrapper = mount(Modal, {
      props: {
        open: true,
        title: 'Error Title',
        message: 'This is an error message',
      },
    });

    expect(wrapper.find('.h').text()).toBe('Error Title');
    expect(wrapper.find('.p').text()).toBe('This is an error message');
  });

  it('should emit close event when close button is clicked', async () => {
    const wrapper = mount(Modal, {
      props: {
        open: true,
        title: 'Test',
        message: 'Message',
      },
    });

    await wrapper.find('.btn').trigger('click');
    expect(wrapper.emitted('close')).toHaveLength(1);
  });

  it('should emit close event when overlay is clicked (not modal)', async () => {
    const wrapper = mount(Modal, {
      props: {
        open: true,
        title: 'Test',
        message: 'Message',
      },
    });

    // Click on overlay - the component has @click.self so it emits close
    const overlay = wrapper.find('.overlay').element as HTMLElement;
    overlay.dispatchEvent(new MouseEvent('click', { bubbles: true }));
    await wrapper.vm.$nextTick();
    expect(wrapper.emitted('close')).toBeTruthy();
  });

  it('should not emit close event when modal content is clicked', async () => {
    const wrapper = mount(Modal, {
      props: {
        open: true,
        title: 'Test',
        message: 'Message',
      },
    });

    // Click on modal content (not overlay background)
    await wrapper.find('.modal').trigger('click');
    expect(wrapper.emitted('close')).toBeUndefined();
  });

  it('should update when props change', async () => {
    const wrapper = mount(Modal, {
      props: {
        open: true,
        title: 'Original Title',
        message: 'Original Message',
      },
    });

    expect(wrapper.find('.h').text()).toBe('Original Title');

    await wrapper.setProps({
      title: 'Updated Title',
      message: 'Updated Message',
    });

    expect(wrapper.find('.h').text()).toBe('Updated Title');
    expect(wrapper.find('.p').text()).toBe('Updated Message');
  });

  it('should handle multiple open/close cycles', async () => {
    const wrapper = mount(Modal, {
      props: {
        open: false,
        title: 'Test',
        message: 'Message',
      },
    });

    // First cycle
    await wrapper.setProps({ open: true });
    expect(wrapper.find('.overlay').exists()).toBe(true);

    await wrapper.find('.btn').trigger('click');
    expect(wrapper.emitted('close')).toHaveLength(1);

    // Second cycle
    await wrapper.setProps({ open: true });
    expect(wrapper.find('.overlay').exists()).toBe(true);

    await wrapper.find('.btn').trigger('click');
    expect(wrapper.emitted('close')).toHaveLength(2);
  });
});
